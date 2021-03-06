using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System.Xml.Linq;
using System.Xml.XPath;


namespace WET.Theme.GCWU.Features.AttachAdapter
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>    
    [Guid("478cb469-ff18-4213-ae6f-11c9b14e3641")]
    public class AttachAdapterEventReceiver : SPFeatureReceiver
    {
        string webPartFQN = "WET.Theme.GCWU.Adapters.RichHtmlFieldAdapter, WET.Theme.GCWU, Version=1.0.0.0, Culture=neutral, PublicKeyToken=04a860f987069351";
        string controlType = "Microsoft.SharePoint.Publishing.WebControls.RichHtmlField";

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            try
            {
                SPSecurity.RunWithElevatedPrivileges(delegate
                {
                    // Add the Mobile Web Part Adapter to the compat.browser file
                    SPWebApplication webApp = null;

                    if (properties.Feature.Parent is SPSite)
                    {
                        SPSite spSite = properties.Feature.Parent as SPSite;
                        webApp = spSite.WebApplication;
                    }
                    else if (properties.Feature.Parent is SPWebApplication)
                    {
                        webApp = properties.Feature.Parent as SPWebApplication;
                    }

                    String pathToCompatBrowser = webApp.IisSettings[SPUrlZone.Default].Path + @"\App_Browsers\compat.browser";
                    XElement compatBrowser = XElement.Load(pathToCompatBrowser);

                    // Get the node for the default browser.
                    XElement controlAdapters = compatBrowser.XPathSelectElement("./browser[@refID = \"default\"]/controlAdapters");

                    // Create and add the markup.
                    XElement newAdapter = new XElement("adapter");

                    newAdapter.SetAttributeValue("controlType", controlType);
                    newAdapter.SetAttributeValue("adapterType", webPartFQN);

                    controlAdapters.Add(newAdapter);
                    compatBrowser.Save(pathToCompatBrowser);
                });
            }
            catch {
                WET.Theme.Intranet.Objects.Logger.WriteLog("Control Adapter - Feature Receiver - Activated");
            }
        }
        
        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            try
            {
                SPWebApplication webApp = null;

                if (properties.Feature.Parent is SPSite)
                {
                    SPSite spSite = properties.Feature.Parent as SPSite;
                    webApp = spSite.WebApplication;
                }
                else if (properties.Feature.Parent is SPWebApplication)
                {
                    webApp = properties.Feature.Parent as SPWebApplication;
                }

                String pathToCompatBrowser = webApp.IisSettings[SPUrlZone.Default].Path + @"\App_Browsers\compat.browser";

                XElement compatBrowser = XElement.Load(pathToCompatBrowser);

                XElement mobileAdapter = compatBrowser.XPathSelectElement(String.Format("./browser[@refID = \"default\"]/controlAdapters/adapter[@controlType = \"{0}\"]", controlType));
                mobileAdapter.Remove();

                // Overwrite the old version of compat.browser with your new version.
                compatBrowser.Save(pathToCompatBrowser);
            }
            catch { }
        }
    }
}
