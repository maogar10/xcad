﻿using NUnit.Framework;
using SolidWorks.Interop.sldworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xarial.XCad.Documents;
using Xarial.XCad.SolidWorks.Documents;

namespace SolidWorks.Tests.Integration
{
    public class DrawingTest : IntegrationTests
    {
        [Test]
        public void ActiveSheetTest() 
        {
            string name;

            using (var doc = OpenDataDocument("Sheets1.SLDDRW"))
            {
                name = (m_App.Documents.Active as ISwDrawing).Sheets.Active.Name;
            }

            Assert.AreEqual("Sheet2", name);
        }

        [Test]
        public void IterateSheetsTest()
        {
            string[] confNames;

            using (var doc = OpenDataDocument("Sheets1.SLDDRW"))
            {
                confNames = (m_App.Documents.Active as ISwDrawing).Sheets.Select(x => x.Name).ToArray();
            }

            Assert.That(confNames.SequenceEqual(new string[] 
            {
                "Sheet1", "Sheet2", "MySheet", "Sheet3"
            }));
        }

        [Test]
        public void GetSheetByNameTest()
        {
            IXSheet sheet1;
            IXSheet sheet2;
            IXSheet sheet3;
            bool r1;
            bool r2;
            Exception e1 = null;

            using (var doc = OpenDataDocument("Sheets1.SLDDRW"))
            {
                var sheets = (m_App.Documents.Active as ISwDrawing).Sheets;

                sheet1 = sheets["Sheet1"];
                r1 = sheets.TryGet("Sheet2", out sheet2);
                r2 = sheets.TryGet("Sheet4", out sheet3);

                try
                {
                    var sheet4 = sheets["Sheet4"];
                }
                catch (Exception ex)
                {
                    e1 = ex;
                }
            }

            Assert.IsNotNull(sheet1);
            Assert.IsNotNull(sheet2);
            Assert.IsNull(sheet3);
            Assert.IsTrue(r1);
            Assert.IsFalse(r2);
            Assert.IsNotNull(e1);
        }

        [Test]
        public void CreateModelViewBasedTest() 
        {
            var refDocPathName = "";
            var viewOrientName = "";

            using (var doc = OpenDataDocument("Selections1.SLDPRT"))
            {
                var partDoc = m_App.Documents.Active as IXDocument3D;
                var view = partDoc.ModelViews[StandardViewType_e.Right];
                
                using(var drw = NewDocument(Interop.swconst.swDocumentTypes_e.swDocDRAWING))
                {
                    var drwDoc = m_App.Documents.Active as ISwDrawing;
                    var drwView = (ISwModelBasedDrawingView)drwDoc.Sheets.Active.DrawingViews.CreateModelViewBased(view);
                    refDocPathName = drwView.DrawingView.ReferencedDocument.GetPathName();
                    viewOrientName = drwView.DrawingView.GetOrientationName();
                }
            }

            Assert.AreEqual(GetFilePath("Selections1.SLDPRT"), refDocPathName);
            Assert.AreEqual("*Right", viewOrientName);
        }

        [Test]
        public void DrawingEventsTest()
        {
            var sheetActiveCount = 0;
            var sheetName = "";

            using (var doc = OpenDataDocument("Sheets1.SLDDRW"))
            {
                var draw = (ISwDrawing)m_App.Documents.Active;

                draw.Sheets.SheetActivated += (d, s) =>
                {
                    sheetName = s.Name;
                    sheetActiveCount++;
                };

                var feat = (IFeature)draw.Drawing.FeatureByName("MySheet");
                feat.Select2(false, -1);
                const int swCommands_Activate_Sheet = 1206;
                m_App.Sw.RunCommand(swCommands_Activate_Sheet, "");
            }

            Assert.AreEqual(1, sheetActiveCount);
            Assert.AreEqual("MySheet", sheetName);
        }

        [Test]
        public void DrawingViewIterateTest() 
        {
            string[] sheet1Views;
            string[] sheet2Views;

            using (var doc = OpenDataDocument("Drawing1\\Drawing1.SLDDRW"))
            {
                var sheets = (m_App.Documents.Active as ISwDrawing).Sheets;
                sheet1Views = sheets["Sheet1"].DrawingViews.Select(v => v.Name).ToArray();
                sheet2Views = sheets["Sheet2"].DrawingViews.Select(v => v.Name).ToArray();
            }

            CollectionAssert.AreEqual(sheet1Views, new string[] { "Drawing View1", "Drawing View2", "Drawing View3" });
            CollectionAssert.AreEqual(sheet2Views, new string[] { "Drawing View4", "Drawing View5" });
        }

        [Test]
        public void DrawingViewDocumentTest()
        {
            string view1DocPath;
            string view2DocPath;
            string view3DocPath;
            string view4DocPath;
            string view5DocPath;

            string view1Conf;
            string view2Conf;
            string view3Conf;
            string view4Conf;
            string view5Conf;

            using (var doc = OpenDataDocument("Drawing1\\Drawing1.SLDDRW"))
            {
                var sheets = (m_App.Documents.Active as ISwDrawing).Sheets;
                
                var v1 = sheets["Sheet1"].DrawingViews["Drawing View1"];
                var v2 = sheets["Sheet1"].DrawingViews["Drawing View2"];
                var v3 = sheets["Sheet1"].DrawingViews["Drawing View3"];
                var v4 = sheets["Sheet2"].DrawingViews["Drawing View4"];
                var v5 = sheets["Sheet2"].DrawingViews["Drawing View5"];

                view1DocPath = v1.ReferencedDocument.Path;
                view2DocPath = v2.ReferencedDocument.Path;
                view3DocPath = v3.ReferencedDocument.Path;
                view4DocPath = v4.ReferencedDocument.Path;
                view5DocPath = v5.ReferencedDocument != null ? v5.ReferencedDocument.Path : "";

                view1Conf = v1.ReferencedConfiguration.Name;
                view2Conf = v2.ReferencedConfiguration.Name;
                view3Conf = v3.ReferencedConfiguration.Name;
                view4Conf = v4.ReferencedConfiguration.Name;
                view5Conf = v5.ReferencedConfiguration != null ? v5.ReferencedConfiguration.Name : "";
            }

            Assert.That(string.Equals(view1DocPath, GetFilePath("Drawing1\\Part1.sldprt"), StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.Equals(view2DocPath, GetFilePath("Drawing1\\Part1.sldprt"), StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.Equals(view3DocPath, GetFilePath("Drawing1\\Part1.sldprt"), StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.Equals(view4DocPath, GetFilePath("Drawing1\\Part2.sldprt"), StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.IsNullOrEmpty(view5DocPath));

            Assert.That(string.Equals(view1Conf, "Conf1", StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.Equals(view2Conf, "Conf1", StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.Equals(view3Conf, "Default", StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.Equals(view4Conf, "Default", StringComparison.CurrentCultureIgnoreCase));
            Assert.That(string.IsNullOrEmpty(view5Conf));
        }
    }
}
