using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class GeometryFeatures : EditorWindow
{
    [MenuItem("Tools/Compute Rotation Features")]
    public static void ProcessCSV()
    {
        string inputFolder = EditorUtility.OpenFolderPanel("Select folder containing CSVs", "", "");
        if (string.IsNullOrEmpty(inputFolder)) return;

        string outputFolder = Path.Combine(inputFolder, "BG_rotation_features");
        Directory.CreateDirectory(outputFolder);

        string[] csvFiles = Directory.GetFiles(inputFolder, "*.csv");

        if (csvFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("No CSVs Found", "No CSV files were found in the selected folder.", "OK");
            return;
        }

        foreach (string inputPath in csvFiles)
        {
            string outputPath = Path.Combine(
                outputFolder,
                Path.GetFileNameWithoutExtension(inputPath) + "_rotation_features.csv"
            );

            string[] lines;
            using (FileStream fs = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader sr = new StreamReader(fs))
            {
                List<string> lineList = new List<string>();
                while (!sr.EndOfStream)
                    lineList.Add(sr.ReadLine());
                lines = lineList.ToArray();
            }

            if (lines.Length < 2) continue;

            string[] headers = lines[0].Split(',');

            int headQuatXIdx = System.Array.IndexOf(headers, "Head_quat_x");
            int headQuatYIdx = System.Array.IndexOf(headers, "Head_quat_y");
            int headQuatZIdx = System.Array.IndexOf(headers, "Head_quat_z");
            int headQuatWIdx = System.Array.IndexOf(headers, "Head_quat_w");

            int headPosXIdx = System.Array.IndexOf(headers, "Head_position_x");
            int headPosYIdx = System.Array.IndexOf(headers, "Head_position_y");
            int headPosZIdx = System.Array.IndexOf(headers, "Head_position_z");

            int rHandPosXIdx = System.Array.IndexOf(headers, "RightHand_position_x");
            int rHandPosYIdx = System.Array.IndexOf(headers, "RightHand_position_y");
            int rHandPosZIdx = System.Array.IndexOf(headers, "RightHand_position_z");

            int lHandPosXIdx = System.Array.IndexOf(headers, "LeftHand_position_x");
            int lHandPosYIdx = System.Array.IndexOf(headers, "LeftHand_position_y");
            int lHandPosZIdx = System.Array.IndexOf(headers, "LeftHand_position_z");

            List<string> outputLines = new List<string>();

            // CHANGED: Added HeadUp_x/y/z to the output header
            outputLines.Add(lines[0] +
                ",HeadForward_x,HeadForward_y,HeadForward_z" +
                ",HeadUp_x,HeadUp_y,HeadUp_z" +
                ",RightHand_Head_local_x,RightHand_Head_local_y,RightHand_Head_local_z" +
                ",LeftHand_Head_local_x,LeftHand_Head_local_y,LeftHand_Head_local_z");

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

                Vector3 headForward = headRotation * Vector3.forward;

                // CHANGED: Compute head up vector from rotation, analogous to headForward
                Vector3 headUp = headRotation * Vector3.up;

                Quaternion headRotationInverse = Quaternion.Inverse(headRotation);
                Vector3 rHand_Head_world = rHandPos - headPos;
                Vector3 lHand_Head_world = lHandPos - headPos;
                Vector3 rHand_Head_local = headRotationInverse * rHand_Head_world;
                Vector3 lHand_Head_local = headRotationInverse * lHand_Head_world;

                // CHANGED: Added headUp components to each output row
                outputLines.Add(lines[i] +
                    $",{headForward.x},{headForward.y},{headForward.z}" +
                    $",{headUp.x},{headUp.y},{headUp.z}" +
                    $",{rHand_Head_local.x},{rHand_Head_local.y},{rHand_Head_local.z}" +
                    $",{lHand_Head_local.x},{lHand_Head_local.y},{lHand_Head_local.z}");
            }

            File.WriteAllLines(outputPath, outputLines);
            Debug.Log($"Processed: {Path.GetFileName(inputPath)} → {outputPath}");
        }

        EditorUtility.DisplayDialog(
            "Done",
            $"Processed {csvFiles.Length} file(s).\nOutput saved to:\n{outputFolder}",
            "OK"
        );
    }
}