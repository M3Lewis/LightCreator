
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Render.ChangeQueue;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Input.Custom;

namespace LightCreator.Class
{
    public class LightType
    {
        public bool Run;
        public int TypeNum;
        public LightType(bool run, int typeNum)
        {
            Run = run;
            TypeNum = typeNum;

        }
        public List<string> SelectLightByType()
        {
            //1.先获取文档中的所有light，添加到一个list
            //2.通过l.LightGeometry.isLight属性判断是否为某种灯光
            //3.将true的物体的编号返回，选出对应的guid，再根据guid选择文档中的light
            Rhino.RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;

            List<Guid> theGuids = new List<Guid>();
            List<string> strList = new List<string>();

            if (Run == false)
            {
                doc.Objects.UnselectAll();
            }
            else
            {
                ObjectEnumeratorSettings objectEnumeratorSettings = new ObjectEnumeratorSettings();
                objectEnumeratorSettings.IncludeLights = true;
                objectEnumeratorSettings.IncludeGrips = false;
                objectEnumeratorSettings.NormalObjects = true;
                objectEnumeratorSettings.LockedObjects = true;
                objectEnumeratorSettings.HiddenObjects = true;
                objectEnumeratorSettings.ReferenceObjects = true;
                objectEnumeratorSettings.ObjectTypeFilter = ObjectType.Light;

                foreach (RhinoObject rhinoObject in RhinoDoc.ActiveDoc.Objects.GetObjectList(objectEnumeratorSettings))
                {


                    var l = rhinoObject as LightObject;

                    /*
                    0.Rectangular lights 矩形灯光
                    1.Spotlights 聚光灯
                    2.Point lights 点光源
                    3.Linear lights 管状灯光
                    4.Directional lights 平行光
                    */

                    switch (TypeNum)
                    {
                        case 0:
                            if (l.LightGeometry.IsRectangularLight == true)
                            {
                                doc.Objects.UnselectAll();

                                theGuids.Add(rhinoObject.Id);
                                doc.Objects.Select(theGuids);
                                
                            }
                            break;
                        case 1:
                            if (l.LightGeometry.IsSpotLight == true)
                            {
                                doc.Objects.UnselectAll();

                                theGuids.Add(rhinoObject.Id);
                                doc.Objects.Select(theGuids);
                             
                            }
                            break;
                        case 2:
                            if (l.LightGeometry.IsPointLight == true)
                            {
                                doc.Objects.UnselectAll();

                                theGuids.Add(rhinoObject.Id);
                                doc.Objects.Select(theGuids);
                            
                            }
                            break;
                        case 3:
                            if (l.LightGeometry.IsLinearLight == true)
                            {
                                doc.Objects.UnselectAll();

                                theGuids.Add(rhinoObject.Id);
                                doc.Objects.Select(theGuids);
                           
                            }
                            break;
                        case 4:
                            if (l.LightGeometry.IsDirectionalLight == true)
                            {
                                doc.Objects.UnselectAll();

                                theGuids.Add(rhinoObject.Id);
                                doc.Objects.Select(theGuids);
                       
                            }
                            break;
                    }
                   
                }
                
            }
            List<string> guidList = new List<string>();
            for (int i = 0; i < theGuids.Count; i++)
            {
                guidList.Add(theGuids[i].ToString());
            }
            return guidList;

        }
    }
}
