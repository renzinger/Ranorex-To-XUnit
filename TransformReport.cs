/*
 * Created by Ranorex
 * User: Ranorex
 * Date: 23.05.2014
 * Time: 9 Uhr
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Threading;
using WinForms = System.Windows.Forms;

using Ranorex;
using Ranorex.Core;
using Ranorex.Core.Testing;
using Ranorex.Libs.Xml;
using System.Xml;

namespace Ranorex.Module
{
    /// <summary>
    /// Description of TransformReport.
    /// </summary>
    [TestModule("25E35AE4-D8C4-4860-A845-F7E92A3CF742", ModuleType.UserCode, 1)]
    public class TransformReport : ITestModule
    {
        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public TransformReport()
        {
            // Do not delete - a parameterless constructor is required!
        }

        /// <summary>
        /// Performs the playback of actions in this module.
        /// </summary>
        /// <remarks>You should not call this method directly, instead pass the module
        /// instance to the <see cref="TestModuleRunner.Run(ITestModule)"/> method
        /// that will in turn invoke this method.</remarks>
        void ITestModule.Run()
        {
            Mouse.DefaultMoveTime = 300;
            Keyboard.DefaultKeyPressTime = 100;
            Delay.SpeedFactor = 1.0;
            
            string xslFile = "ranorex-to-xunit.xsl";
            string xUnitFile = System.IO.Path.GetFileNameWithoutExtension(Ranorex.Core.Reporting.TestReport.ReportEnvironment.ReportName)+"_xunit.xml";
            
            if (System.IO.File.Exists (xslFile))
            {

            	var item = Ranorex.Core.Reporting.ActivityStack.Instance.RootActivity;
            	string documentStr;
            	using (var ms = new System.IO.MemoryStream())
		{
            		var sw = new System.IO.StreamWriter(ms, new UTF8Encoding(false));
		 	var xmlPrinter = XmlPrinter.BeginDocument(sw, false);
	 		xmlPrinter.Print(item).CompleteDocument();
	 	 	documentStr = new UTF8Encoding(false).GetString(ms.ToArray());
		}
				            	
            	XmlDocument xmlDoc = new XmlDocument ();           	
            	xmlDoc.LoadXml (documentStr);
            	
            	// Delete Log-Information from current Module (based on GUID)
            	System.Xml.XmlNode currentNode = xmlDoc.SelectSingleNode("//activity[@moduleid=\"25e35ae4-d8c4-4860-a845-f7e92a3cf742\"]");
            	if (currentNode != null)
            		currentNode.ParentNode.RemoveChild(currentNode);

            	// Save report to stream
		using (System.IO.MemoryStream stream= new System.IO.MemoryStream())
            	{
	        	xmlDoc.Save (stream);
	            	
	            	// Load the style sheet
	            	System.Xml.Xsl.XslCompiledTransform xslTrans = new System.Xml.Xsl.XslCompiledTransform();
			xslTrans.Load(xslFile);
	
	    		// Perform Transformation
	    		stream.Position =0;
	    		using (System.Xml.XmlReader reader= System.Xml.XmlReader.Create(stream))
		    	{
		    		System.Xml.XmlWriter writer = new System.Xml.XmlTextWriter(xUnitFile, new UTF8Encoding(false)); 
		    		writer.WriteStartDocument();
		      		xslTrans.Transform(reader, null, writer);
		    	}
            	}
            	Ranorex.Report.Info(string.Format("Transformed Report File to xUnit-Report: {0}", xUnitFile));
            }
            else
            {
            	Ranorex.Report.Info (string.Format("Error -- XSL file {0} for xUnit transformation not found", xslFile));
            }
        }
    }
}
