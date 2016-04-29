/****************************************************************************
Copyright (c) 2013-2014,Dalian-LingYOU tech.
 This is not a free-ware .DO NOT use it without any authorization.
 * 
 * date : 2016/4/18 10:37:03
 * author : Labor
 * purpose : �Զ��и��
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
        [MenuItem("liubo/�Զ��и��������")]
        public static void AutoClip()
        {
            string path = Selection.activeObject == null ? "" : AssetDatabase.GetAssetPath(Selection.activeObject);
            Debug.Log("[�и��] path=" + path);

            ModelImporter mi = AssetImporter.GetAtPath(path) as ModelImporter;
            if (mi == null)
            {
                Debug.Log("[�и��] û�ҵ�FBX�ļ��� path=" + path);
            }
            else
            {
                Debug.Log("[�и��] ��ʼ�иpath=" + path);

                // ע�Ᵽ���Ͷ�����һЩ���ݣ�����󶨵�event

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

        [MenuItem("liubo/�Զ��и��������2")]
        public static void AutoClip2()
        {
            /*
             *  ��ȡexcel����ȡ��������Ϣ�����֣���ʼ֡������֡
             *  �ٶ�ȡfbx�Ķ�����Ϣ����������ͬ�Ľ�����Ϣmerge
             *      merge����ʼ֡������֡     
             *      ������event��Ϣ��mask��Ϣ��curve��Ϣ
             *      ������¼ӵģ���ô�����µ���Ϣ
             * **/

            // ��ȡexcel


            // ��ȡm_AnimationClip��Ȼ����

            ModelImporter modelImporter = Selection.activeObject == null ? null : (ModelImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(Selection.activeObject));
            SerializedObject serializedObject = modelImporter == null ? null : new SerializedObject(modelImporter);
            //if (serializedObject != null)
            //{
            //    Debug.Log("[�и��] �и����" + serializedObject.targetObject.name);
            //    SerializedProperty attr = serializedObject.GetIterator();
            //    attr.Next(true);
            //    do
            //    {
            //        Debug.Log("[�и��] attr-name=" + attr.name);
            //    }
            //    while (attr.Next(true));
            //}
            SerializedProperty m_ClipAnimations = serializedObject == null ? null : serializedObject.FindProperty("m_ClipAnimations");
            if (m_ClipAnimations != null)
            {
                if (m_ClipAnimations.arraySize > 0)
                {
                    //AnimationClip ac = m_ClipAnimations.GetArrayElementAtIndex(0).serializedObject.targetObject as AnimationClip;
                    //Debug.Log("[�и��] ac=" + (ac == null ? "none" : ac.name) + ", ref-obj=" + m_ClipAnimations.GetArrayElementAtIndex(0).serializedObject.targetObject.name);

                    // ������ص��� ModelImporterClipAnimation ����
                    SerializedProperty sp = m_ClipAnimations.GetArrayElementAtIndex(0);

                    //ShowProperties(sp);
                    Debug.Log("[�и��] ac.name=" + sp.FindPropertyRelative("name").stringValue);

                    // ����������Ϣ
                    // �޸�event
                    {
                        sp.FindPropertyRelative("firstFrame").floatValue = 10;
                        sp.FindPropertyRelative("lastFrame").floatValue = 20;
                    }


                    SerializedProperty sp_event = sp.FindPropertyRelative("events");
                    if (sp_event != null && sp_event.arraySize > 0)
                    {
                        Debug.Log("[�и��] sp_event.arraySize=" + sp_event.arraySize);

                        // �޸�event
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
                Debug.Log("[��ʾ����] sp.name=" + sp.displayName + ", " + sp.name);
                sp.Next(true);
                do
                {
                    Debug.Log("[��ʾ����] sp.name=" + sp.displayName + ", " + sp.name);
                } while (sp.Next(false));
            }
        }

        Dictionary<string, ConfigFile> ReadXML(string path)
        {
            //path = Application.dataPath + "/../Builder/builder_config.xls";

            Dictionary<string, ConfigFile> ret = new Dictionary<string, ConfigFile>();

            // !< ˵��:�ͻ��˵ı��ȡ,�Ͳ��߷����������ĸ��Ӵ�����..
            HSSFWorkbook wk = new HSSFWorkbook(File.OpenRead(path));
            HSSFSheet sheet = null;
            string sheetname = "";
            for (int a = 0; a < wk.NumberOfSheets; ++a)
            {
                sheet = wk.GetSheetAt(a) as HSSFSheet;
                sheetname = wk.GetSheetName(a);
                if (sheet == null)
                {
                    Debug.LogError("�쳣!" + wk.GetFullName());
                    continue;
                }
                if (ret.ContainsKey(sheetname))
                {
                    Debug.LogError("��excel���sheet�����ظ��ˣ�" + wk.GetSheetName(a));
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
                                // !< ������?
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
