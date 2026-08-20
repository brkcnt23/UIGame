#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Cuts generated quest notes out of their background.
///
/// The image generator surrounds each note with a dark atmospheric vignette
/// instead of the flat white it was asked for. Pinned to a board that halo
/// shows as a dark rectangle around the paper, so the paper has to be
/// isolated before the art is usable.
///
/// The paper is reliably lighter and less saturated than the surround, which
/// is enough to separate them: threshold on brightness, keep the largest
/// connected region, fill its holes, then trim to that region's bounds.
///
/// Originals are left untouched — output goes to a Cut subfolder, so a bad
/// threshold costs nothing.
///
/// Tools > UIGame > Quests > Cut notes from background
/// </summary>
public static class QuestNoteCutter
{
    private const string DefaultFolder = "Assets/UI Elements/Quests";

    [MenuItem("Tools/UIGame/Quests/Cut notes from background")]
    public static void CutAll()
    {
        string folder = GetTargetFolder();
        if (folder == null) return;

        string outFolder = Path.Combine(folder, "Cut").Replace('\\', '/');
        if (!AssetDatabase.IsValidFolder(outFolder))
            AssetDatabase.CreateFolder(folder, "Cut");

        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
        int done = 0, failed = 0;

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // Skip anything already produced by a previous run.
            if (path.Contains("/Cut/")) continue;

            if (Cut(path, outFolder)) done++;
            else failed++;
        }

        AssetDatabase.Refresh();

        Debug.Log($"[QuestCutter] {done} note(s) cut into {outFolder}. " +
                  (failed > 0 ? $"{failed} failed — check they are readable." : "") +
                  "\nOriginals untouched. Delete them once you are happy with the cuts.");
    }

    private static bool Cut(string assetPath, string outFolder)
    {
        var readable = LoadReadable(assetPath);
        if (readable == null) return false;

        int w = readable.width, h = readable.height;
        var pixels = readable.GetPixels32();

        // Background brightness, sampled from the outer border.
        float bg = BorderBrightness(pixels, w, h);
        float threshold = bg + 0.11f;

        var mask = new bool[w * h];
        for (int i = 0; i < pixels.Length; i++)
        {
            var p = pixels[i];
            float r = p.r / 255f, g = p.g / 255f, b = p.b / 255f;

            float lum = (r + g + b) / 3f;
            float max = Mathf.Max(r, Mathf.Max(g, b));
            float min = Mathf.Min(r, Mathf.Min(g, b));
            float sat = max > 0f ? (max - min) / max : 0f;

            // Paper is bright and comparatively grey; the vignette is dark and
            // strongly orange.
            mask[i] = lum > threshold && sat < 0.62f;
        }

        KeepLargestRegion(mask, w, h);
        FillHoles(mask, w, h);

        if (!Bounds(mask, w, h, out int x0, out int y0, out int x1, out int y1))
        {
            Object.DestroyImmediate(readable);
            return false;
        }

        int cw = x1 - x0 + 1, ch = y1 - y0 + 1;
        var outPixels = new Color32[cw * ch];

        for (int y = 0; y < ch; y++)
        {
            for (int x = 0; x < cw; x++)
            {
                int src = (y0 + y) * w + (x0 + x);
                var p = pixels[src];
                outPixels[y * cw + x] = new Color32(p.r, p.g, p.b, mask[src] ? (byte)255 : (byte)0);
            }
        }

        var result = new Texture2D(cw, ch, TextureFormat.RGBA32, false);
        result.SetPixels32(outPixels);
        result.Apply();

        string name = Path.GetFileNameWithoutExtension(assetPath);
        string outPath = $"{outFolder}/{name}.png";
        File.WriteAllBytes(outPath, result.EncodeToPNG());

        Object.DestroyImmediate(readable);
        Object.DestroyImmediate(result);
        return true;
    }

    // -----------------------------------------------------------------

    private static float BorderBrightness(Color32[] pixels, int w, int h)
    {
        var samples = new List<float>();

        void Sample(int x, int y)
        {
            var p = pixels[y * w + x];
            samples.Add((p.r + p.g + p.b) / 765f);
        }

        for (int x = 0; x < w; x += 4)
        {
            Sample(x, 0);
            Sample(x, h - 1);
        }

        for (int y = 0; y < h; y += 4)
        {
            Sample(0, y);
            Sample(w - 1, y);
        }

        samples.Sort();
        return samples.Count == 0 ? 0.3f : samples[samples.Count / 2];
    }

    /// <summary>
    /// Flood fill from every unvisited true pixel, keeping only the biggest
    /// region. Text and torn edges make isolated specks; the paper is the one
    /// large blob.
    /// </summary>
    private static void KeepLargestRegion(bool[] mask, int w, int h)
    {
        var label = new int[mask.Length];
        var sizes = new List<int> { 0 };
        var stack = new Stack<int>();

        for (int start = 0; start < mask.Length; start++)
        {
            if (!mask[start] || label[start] != 0) continue;

            int id = sizes.Count;
            int size = 0;

            stack.Push(start);
            label[start] = id;

            while (stack.Count > 0)
            {
                int i = stack.Pop();
                size++;

                int x = i % w, y = i / w;

                void Try(int nx, int ny)
                {
                    if (nx < 0 || ny < 0 || nx >= w || ny >= h) return;

                    int n = ny * w + nx;
                    if (!mask[n] || label[n] != 0) return;

                    label[n] = id;
                    stack.Push(n);
                }

                Try(x - 1, y); Try(x + 1, y); Try(x, y - 1); Try(x, y + 1);
            }

            sizes.Add(size);
        }

        int best = 0, bestSize = 0;
        for (int i = 1; i < sizes.Count; i++)
        {
            if (sizes[i] <= bestSize) continue;
            bestSize = sizes[i];
            best = i;
        }

        for (int i = 0; i < mask.Length; i++)
            mask[i] = label[i] == best;
    }

    /// <summary>
    /// Fills enclosed gaps — dark ink and the drawing sit inside the paper and
    /// fail the brightness test, but they are part of the note.
    /// </summary>
    private static void FillHoles(bool[] mask, int w, int h)
    {
        var outside = new bool[mask.Length];
        var stack = new Stack<int>();

        void Seed(int x, int y)
        {
            int i = y * w + x;
            if (mask[i] || outside[i]) return;

            outside[i] = true;
            stack.Push(i);
        }

        for (int x = 0; x < w; x++) { Seed(x, 0); Seed(x, h - 1); }
        for (int y = 0; y < h; y++) { Seed(0, y); Seed(w - 1, y); }

        while (stack.Count > 0)
        {
            int i = stack.Pop();
            int x = i % w, y = i / w;

            void Try(int nx, int ny)
            {
                if (nx < 0 || ny < 0 || nx >= w || ny >= h) return;

                int n = ny * w + nx;
                if (mask[n] || outside[n]) return;

                outside[n] = true;
                stack.Push(n);
            }

            Try(x - 1, y); Try(x + 1, y); Try(x, y - 1); Try(x, y + 1);
        }

        for (int i = 0; i < mask.Length; i++)
            if (!outside[i]) mask[i] = true;
    }

    private static bool Bounds(bool[] mask, int w, int h,
                               out int x0, out int y0, out int x1, out int y1)
    {
        x0 = w; y0 = h; x1 = -1; y1 = -1;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (!mask[y * w + x]) continue;

                if (x < x0) x0 = x;
                if (x > x1) x1 = x;
                if (y < y0) y0 = y;
                if (y > y1) y1 = y;
            }
        }

        return x1 >= x0 && y1 >= y0;
    }

    /// <summary>
    /// Imported textures are compressed and non-readable by default, so the
    /// importer settings are flipped for the duration of the read.
    /// </summary>
    private static Texture2D LoadReadable(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return null;

        bool wasReadable = importer.isReadable;
        var wasCompression = importer.textureCompression;

        if (!wasReadable || wasCompression != TextureImporterCompression.Uncompressed)
        {
            importer.isReadable = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        var source = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (source == null) return null;

        var copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        copy.SetPixels32(source.GetPixels32());
        copy.Apply();

        if (!wasReadable || wasCompression != TextureImporterCompression.Uncompressed)
        {
            importer.isReadable = wasReadable;
            importer.textureCompression = wasCompression;
            importer.SaveAndReimport();
        }

        return copy;
    }

    private static string GetTargetFolder()
    {
        if (Selection.activeObject != null)
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (AssetDatabase.IsValidFolder(path)) return path;
        }

        if (AssetDatabase.IsValidFolder(DefaultFolder)) return DefaultFolder;

        Debug.LogError($"[QuestCutter] Select the folder with the notes, or put them in {DefaultFolder}.");
        return null;
    }
}
#endif
