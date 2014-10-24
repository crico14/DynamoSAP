﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.IO;

using SAPConnection;

using DynamoSAP.Structure;
using DynamoSAP.Analysis;

//DYNAMO
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

//SAP 
using SAP2000v16;

namespace DynamoSAP.Assembly
{
    public class SAPModel
    {
        private static cSapModel mySapModel;

        //// PRIVATE METHODS ////
        #region
        //CREATE FRAME METHOD
        private static void CreateFrame(Frame f, ref cSapModel mySapModel)
        {
            // Draw Frm Object return Label
            string dummy = string.Empty;
            //1. Create Frame
            SAPConnection.StructureMapper.DrawFrm(ref mySapModel,f.BaseCrv.StartPoint.X,
                f.BaseCrv.StartPoint.Y,
                f.BaseCrv.StartPoint.Z,
                f.BaseCrv.EndPoint.X,
                f.BaseCrv.EndPoint.Y,
                f.BaseCrv.EndPoint.Z,
                ref dummy);

            // TODO: set custom name !
            f.Label = dummy; // for now passing the SAP label to Frame label!
            
            // 2. Set GUID
            SAPConnection.StructureMapper.SetGUIDFrm(ref mySapModel, f.Label, f.GUID);

            // 3. Get or Define Section Profile
            bool exists = SAPConnection.StructureMapper.IsSectionExistsFrm(ref mySapModel, f.SectionProfile);
            if (!exists) // if doesnot exists define new sec property
            {
                string MatProp = SAPConnection.MaterialMapper.DynamoToSap(f.Material);
                string SecCatalog = "AISC14.pro"; // US_Imperial TODO: ASK TO USER ?
                //define new section property
                SAPConnection.StructureMapper.DefinePropFrm(ref mySapModel, f.SectionProfile, MatProp, SecCatalog, f.SectionProfile);
            }
            //Assign section profile toFrame
            SAPConnection.StructureMapper.SetSectionFrm(ref mySapModel, f.Label, f.SectionProfile);

            // 3. Set Justification TODO: Vertical & Lateral Justification
            SAPConnection.JustificationMapper.DynamoToSAPFrm(ref mySapModel, f.Label, f.Justification); // TO DO: lateral and vertical justificaton

            // 4. Set Rotation
            SAPConnection.JustificationMapper.SetRotationFrm(ref mySapModel, f.Label, f.Rotation);

        }

        
        #endregion



        //// DYNAMO NODES ////
        public static string CreateSAPModel(List<Element> SAPElements, List<LoadPattern> SAPLoadPatterns )
        {
            //1. Instantiate SAPModel
            SAP2000v16.SapObject mySapObject = null;

            try
            {
                SAPConnection.Initialize.InitializeSapModel(ref mySapObject, ref mySapModel);
            }
            catch (Exception)
            {
                SAPConnection.Initialize.Release(ref mySapObject, ref mySapModel);
            };


            //2. Create Geometry
            foreach (var el in SAPElements)
            {
                if (el.GetType().ToString().Contains("Frame"))
                {
                    CreateFrame(el as Frame, ref mySapModel);
                }
            }



            // 3. Assigns Restraints to Nodes



            // 4. Add Load Patterns

            foreach (LoadPattern lp in SAPLoadPatterns)
            {
                //Call the AddLoadPattern method
                SAPConnection.LoadMapper.AddLoadPattern(ref mySapModel, lp.Name, lp.Type, lp.Multiplier);          
            }

            // 5. Define Load Cases
            


            // 6. Loads 
            //foreach (Load load in SAPLoads)
            //{
            //    if (load.LoadType == "PointLoad")
            //    {
            //        //Call the CreatePointLoad method
                    
            //        //ret=SAPConnection.LoadMapper.CreatePointLoad(ref mySapModel, load.FrameName, load.LoadPat, load.MyType, load.Dir, load.Dist, load.Val, load.CSys, load.RelDist, load.Replace);
            //    }
            //    if (load.LoadType == "DistributedLoad")
            //    {
            //        //Call the CreateDistributedLoad method

            //        //ret = SAPConnection.LoadMapper.CreateDistributedLoad(ref mySapModel, load.FrameName, load.LoadPat, load.MyType, load.Dir, load.Dist, load.Dist2, load.Val, load.Val2, load.CSys, load.RelDist, load.Replace);
            //    }
            //}

            //if can't set to null, will be a hanging process
            mySapModel = null;
            mySapObject = null;

            return "Success";
        }

        // PRIVATE CONSTRUCTOR
        private SAPModel() { }

    }
}
