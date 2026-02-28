using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class GeometryFeatures : EditorWindow
{
    // Adds menu item
    [MenuItem("Tools/Compute Rotation Features")]

    public static void ProcessCSV()
    {
        // CHANGED: Select a folder instead of a single file for batch processing
        string inputFolder = EditorUtility.OpenFolderPanel("Select folder containing CSVs", "", "");
        if (string.IsNullOrEmpty(inputFolder)) return;

        // CHANGED: Create a new output subfolder inside the selected input folder
        string outputFolder = Path.Combine(inputFolder, "BG_rotation_features");
        Directory.CreateDirectory(outputFolder);

        // CHANGED: Get all CSV files in the selected folder
        string[] csvFiles = Directory.GetFiles(inputFolder, "*.csv");

        if (csvFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("No CSVs Found", "No CSV files were found in the selected folder.", "OK");
            return;
        }

        // CHANGED: Loop over every CSV file found in the folder
        foreach (string inputPath in csvFiles)
        {
            // CHANGED: Output file goes to the new subfolder, not alongside the input
            string outputPath = Path.Combine(
                outputFolder,
                Path.GetFileNameWithoutExtension(inputPath) + "_rotation_features.csv"
            );

            string[] lines = File.ReadAllLines(inputPath);
            if (lines.Length < 2) continue; // skip empty files

            string[] headers = lines[0].Split(',');

            // CHANGED: Updated column names to match actual CSV headers
            int headQuatXIdx = System.Array.IndexOf(headers, "Head_quat_x");
            int headQuatYIdx = System.Array.IndexOf(headers, "Head_quat_y");
            int headQuatZIdx = System.Array.IndexOf(headers, "Head_quat_z");
            int headQuatWIdx = System.Array.IndexOf(headers, "Head_quat_w");

            int headPosXIdx = System.Array.IndexOf(headers, "Head_position_x");
            int headPosYIdx = System.Array.IndexOf(headers, "Head_position_y");
            int headPosZIdx = System.Array.IndexOf(headers, "Head_position_z");

            // CHANGED: Updated from RHand/LHand to RightHand/LeftHand to match actual CSV headers
            int rHandPosXIdx = System.Array.IndexOf(headers, "RightHand_position_x");
            int rHandPosYIdx = System.Array.IndexOf(headers, "RightHand_position_y");
            int rHandPosZIdx = System.Array.IndexOf(headers, "RightHand_position_z");

            int lHandPosXIdx = System.Array.IndexOf(headers, "LeftHand_position_x");
            int lHandPosYIdx = System.Array.IndexOf(headers, "LeftHand_position_y");
            int lHandPosZIdx = System.Array.IndexOf(headers, "LeftHand_position_z");

            // Build output
            List<string> outputLines = new List<string>();

            outputLines.Add(lines[0] +
                ",HeadForward_x,HeadForward_y,HeadForward_z" +
                ",RightHand_Head_local_x,RightHand_Head_local_y,RightHand_Head_local_z" +
                ",LeftHand_Head_local_x,LeftHand_Head_local_y,LeftHand_Head_local_z");

            // Process each data row
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] cols = lines[i].Split(',');

                Quaternion headRotation = new Quaternion(
                    float.Parse(cols[headQuatXIdx]),
                    float.Parse(cols[headQuatYIdx]),
                    float.Parse(cols[headQuatZIdx]),
                    float.Parse(cols[headQuatWIdx])
                );

                Vector3 headPos = new Vector3(
                    float.Parse(cols[headPosXIdx]),
                    float.Parse(cols[headPosYIdx]),
                    float.Parse(cols[headPosZIdx])
                );

                Vector3 rHandPos = new Vector3(
                    float.Parse(cols[rHandPosXIdx]),
                    float.Parse(cols[rHandPosYIdx]),
                    float.Parse(cols[rHandPosZIdx])
                );

                Vector3 lHandPos = new Vector3(
                    float.Parse(cols[lHandPosXIdx]),
                    float.Parse(cols[lHandPosYIdx]),
                    float.Parse(cols[lHandPosZIdx])
                );

                // Compute features
                Vector3 headForward = headRotation * Vector3.forward;

                Quaternion headRotationInverse = Quaternion.Inverse(headRotation);
                Vector3 rHand_Head_world = rHandPos - headPos;
                Vector3 lHand_Head_world = lHandPos - headPos;
                Vector3 rHand_Head_local = headRotationInverse * rHand_Head_world;
                Vector3 lHand_Head_local = headRotationInverse * lHand_Head_world;

                outputLines.Add(lines[i] +
                    $",{headForward.x},{headForward.y},{headForward.z}" +
                    $",{rHand_Head_local.x},{rHand_Head_local.y},{rHand_Head_local.z}" +
                    $",{lHand_Head_local.x},{lHand_Head_local.y},{lHand_Head_local.z}");
            }

            File.WriteAllLines(outputPath, outputLines);
            Debug.Log($"Processed: {Path.GetFileName(inputPath)} → {outputPath}");
        }

        // CHANGED: Final dialog reports how many files were processed and where the output folder is
        EditorUtility.DisplayDialog(
            "Done",
            $"Processed {csvFiles.Length} file(s).\nOutput saved to:\n{outputFolder}",
            "OK"
        );
    }
}