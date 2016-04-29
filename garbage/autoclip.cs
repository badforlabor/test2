/****************************************************************************
Copyright (c) 2013-2014,Dalian-LingYOU tech.
 This is not a free-ware .DO NOT use it without any authorization.
 * 
 * date : 2016/4/18 10:37:03
 * author : Labor
 * purpose : 自动切割动画
****************************************************************************/
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NPOI.HSSF.UserModel;

namespace MLGame
{
    static class SerializedPropertyExtension
    {
        public static void SetStringValue(this SerializedProperty self, string propertyName, string value)
        {
            self.FindPropertyRelative(propertyName).stringValue = value;
        }
    }


    class AnimationAutoClip
    {
        [MenuItem("liubo/自动切割骨骼动画")]
        public static void AutoClip()
        {
            string path = Selection.activeObject == null ? "" : AssetDatabase.GetAssetPath(Selection.activeObject);
            Debug.Log("[切割动画] path=" + path);

            ModelImporter mi = AssetImporter.GetAtPath(path) as ModelImporter;
            if (mi == null)
            {
                Debug.Log("[切割动画] 没找到FBX文件！ path=" + path);
            }
            else
            {
                Debug.Log("[切割动画] 开始切割：path=" + path);

                // 注意保留就动画的一些数据，比如绑定的event

                List<ModelImporterClipAnimation> oldClips = new List<ModelImporterClipAnimation>();
                List<ModelImporterClipAnimation> newClips = new List<ModelImporterClipAnimation>();
                if (mi.clipAnimations != null)
                {
                    oldClips.AddRange(mi.clipAnimations);
                }

#if true//TEST
                //AnimationUtility.GetAnimationClipSettings(null);
                ModelImporterClipAnimation test = new ModelImporterClipAnimation();
                test.name = "test";
                test.takeName = "auto";
                test.firstFrame = 0;
                test.lastFrame = 30;
                test.wrapMode = WrapMode.Default;
                test.keepOriginalPositionY = true;
                test.loop = false;
                test.loopTime = false;
                test.loopPose = false;
                test.keepOriginalOrientation = false;
                test.keepOriginalPositionY = false;
                test.keepOriginalPositionXZ = false;
                test.cycleOffset = 0;
                test.heightOffset = 0;
                test.rotationOffset = 0;

                newClips.Add(test);
                newClips.AddRange(oldClips);
#endif

                mi.clipAnimations = newClips.ToArray();
                EditorUtility.SetDirty(Selection.activeObject);


            }
        }

        [MenuItem("liubo/自动切割骨骼动画2")]
        public static void AutoClip2()
        {
            /*
             *  读取excel表，获取动画的信息：名字，起始帧，结束帧
             *  再读取fbx的动画信息，对名字相同的进行信息merge
             *      merge：起始帧，结束帧     
             *      保留：event信息，mask信息，curve信息
             *      如果有新加的，那么加入新的信息
             * **/

            // 读取excel


            // 获取m_AnimationClip，然后处理

            ModelImporter modelImporter = Selection.activeObject == null ? null : (ModelImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(Selection.activeObject));
            SerializedObject serializedObject = modelImporter == null ? null : new SerializedObject(modelImporter);
            //if (serializedObject != null)
            //{
            //    Debug.Log("[切割动画] 切割对象：" + serializedObject.targetObject.name);
            //    SerializedProperty attr = serializedObject.GetIterator();
            //    attr.Next(true);
            //    do
            //    {
            //        Debug.Log("[切割动画] attr-name=" + attr.name);
            //    }
            //    while (attr.Next(true));
            //}
            SerializedProperty m_ClipAnimations = serializedObject == null ? null : serializedObject.FindProperty("m_ClipAnimations");
            if (m_ClipAnimations != null)
            {
                if (m_ClipAnimations.arraySize > 0)
                {
                    //AnimationClip ac = m_ClipAnimations.GetArrayElementAtIndex(0).serializedObject.targetObject as AnimationClip;
                    //Debug.Log("[切割动画] ac=" + (ac == null ? "none" : ac.name) + ", ref-obj=" + m_ClipAnimations.GetArrayElementAtIndex(0).serializedObject.targetObject.name);

                    // 这个返回的是 ModelImporterClipAnimation 对象
                    SerializedProperty sp = m_ClipAnimations.GetArrayElementAtIndex(0);

                    //ShowProperties(sp);
                    Debug.Log("[切割动画] ac.name=" + sp.FindPropertyRelative("name").stringValue);

                    // 更改他的信息
                    // 修改event
                    {
                        sp.FindPropertyRelative("firstFrame").floatValue = 10;
                        sp.FindPropertyRelative("lastFrame").floatValue = 20;
                    }


                    SerializedProperty sp_event = sp.FindPropertyRelative("events");
                    if (sp_event != null && sp_event.arraySize > 0)
                    {
                        Debug.Log("[切割动画] sp_event.arraySize=" + sp_event.arraySize);

                        // 修改event
                        if (false)
                        {
                            sp_event.InsertArrayElementAtIndex(sp_event.arraySize);
                            SerializedProperty sp_one_event = sp_event.GetArrayElementAtIndex(sp_event.arraySize - 1);
                            sp_one_event.SetStringValue("functionName", "123");
                        }
                    }
                    serializedObject.ApplyModifiedProperties();
                    AssetDatabase.WriteImportSettingsIfDirty(AssetDatabase.GetAssetPath(Selection.activeObject));
                }
                EditorUtility.SetDirty(Selection.activeObject);
                AssetDatabase.SaveAssets();
            }


        }


        static void ShowProperties(SerializedProperty sp)
        {
            SerializedProperty backup = sp.Copy();

            {
                sp = backup.Copy();
                Debug.Log("[显示属性] sp.name=" + sp.displayName + ", " + sp.name);
                sp.Next(true);
                do
                {
                    Debug.Log("[显示属性] sp.name=" + sp.displayName + ", " + sp.name);
                } while (sp.Next(false));
            }
        }

        Dictionary<string, ConfigFile> ReadXML(string path)
        {
            //path = Application.dataPath + "/../Builder/builder_config.xls";

            Dictionary<string, ConfigFile> ret = new Dictionary<string, ConfigFile>();

            // !< 说明:客户端的表读取,就不走服务器那样的复杂处理啦..
            HSSFWorkbook wk = new HSSFWorkbook(File.OpenRead(path));
            HSSFSheet sheet = null;
            string sheetname = "";
            for (int a = 0; a < wk.NumberOfSheets; ++a)
            {
                sheet = wk.GetSheetAt(a) as HSSFSheet;
                sheetname = wk.GetSheetName(a);
                if (sheet == null)
                {
                    Debug.LogError("异常!" + wk.GetFullName());
                    continue;
                }
                if (ret.ContainsKey(sheetname))
                {
                    Debug.LogError("该excel表的sheet名字重复了：" + wk.GetSheetName(a));
                }
                else
                {
                    ConfigFile result = new ConfigFile(wk.GetFullName() + "_" + sheetname);
                    List<String> header = new List<string>();
                    for (int i = sheet.FirstRowNum; i <= sheet.LastRowNum; ++i)
                    {
                        HSSFRow row = sheet.GetRow(i) as HSSFRow;
                        if (row == null) continue;
                        ArrayList line = new ArrayList();
                        for (int j = row.FirstCellNum; j <= row.LastCellNum; ++j)
                        {
                            HSSFCell cell = row.GetCell(j) as HSSFCell;
                            if (cell == null) continue;
                            if (i == sheet.FirstRowNum)
                            {
                                header.Add(cell.ToString());
                            }
                            else
                            {
                                line.Add(cell.ToString());
                            }
                        }
                        if (i == sheet.FirstRowNum)
                        {
                            result.SetTitles(header.ToArray());
                        }
                        else
                        {
                            if (line.Count > 0)
                            {
                                result.AddData(line[0] as String, line);
                            }
                            else
                            {
                                // !< 读完了?
                                break;
                            }
                        }
                    }
                    ret.Add(sheetname, result);
                }
            }

            return ret;
        }
    }
}
