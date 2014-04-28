﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace steamBackup.AppServices.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("steamBackup.AppServices.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Absolute path needed, not relative.
        /// </summary>
        public static string AbsolutePathExceptionText {
            get {
                return ResourceManager.GetString("AbsolutePathExceptionText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Programmers:
        ///Brian &apos;Du-z&apos; Duhs (overclock.net/u/69975/du-z)
        ///Juergen &apos;UniqProject&apos; Tem (overclock.net/u/395443/uniqproject)
        ///James &apos;FiX&apos; Warner (overclock.net/u/98516/fix)
        ///
        ///Help and Sugestions:
        ///InsaneMatt (overclock.net/u/106953/insanematt)
        ///d3viliz3d (overclock.net/u/167561/d3viliz3d)
        ///davcomNZ (overclock.net/u/261011/davcomnz)
        ///ToonLink15 (overclock.net/u/366419/toonlink15).
        /// </summary>
        public static string AppCredits {
            get {
                return ResourceManager.GetString("AppCredits", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Listed below are the errors for a backup or restore.
        ///
        ///Please try running the backup process again making sure that there are no programs accessing the files being backed up (e.g. Steam).
        ///
        ///To check the integrity of this backup: navigate to the backup location -&gt; Select all files in the &apos;common&apos; folder -&gt; right click -&gt; 7zip -&gt; Test archive. You should do the same for &apos;Source Games.7z&apos; also.
        ///.
        /// </summary>
        public static string ErrorListHeader {
            get {
                return ResourceManager.GetString("ErrorListHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Time: {0}, ETA: {1}.
        /// </summary>
        public static string EtaFormatStr {
            get {
                return ResourceManager.GetString("EtaFormatStr", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ETA: {1}.
        /// </summary>
        public static string EtaShortFormatStr {
            get {
                return ResourceManager.GetString("EtaShortFormatStr", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid path.
        /// </summary>
        public static string InvalidPathExceptionText {
            get {
                return ResourceManager.GetString("InvalidPathExceptionText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The job was canceled by the user, there is a potential for file corruption..
        /// </summary>
        public static string JobCanceledUser {
            get {
                return ResourceManager.GetString("JobCanceledUser", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///Job Details:
        ///{0}.
        /// </summary>
        public static string JobErrorDetails {
            get {
                return ResourceManager.GetString("JobErrorDetails", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///Error Message:
        ///{0}.
        /// </summary>
        public static string JobErrorMsg {
            get {
                return ResourceManager.GetString("JobErrorMsg", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///Stack Trace:
        ///{0}.
        /// </summary>
        public static string JobErrorStack {
            get {
                return ResourceManager.GetString("JobErrorStack", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        /////////////////////// Error Time: {0} \\\\\\\\\\\\\\\\\\\\
        ///.
        /// </summary>
        public static string JobErrorTime {
            get {
                return ResourceManager.GetString("JobErrorTime", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The percent of finished work must be between 0 and 100..
        /// </summary>
        public static string PercentRangeError {
            get {
                return ResourceManager.GetString("PercentRangeError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Jobs started: {0} of {1}
        ///Jobs skipped: {2} of {3}
        ///Jobs total: {4} of {5}.
        /// </summary>
        public static string ProgressFormatStr {
            get {
                return ResourceManager.GetString("ProgressFormatStr", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Speed: {0:###0} KB/s, {1}.
        /// </summary>
        public static string SpeedKBFormatStr {
            get {
                return ResourceManager.GetString("SpeedKBFormatStr", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Speed: {0:###0} MB/s, {1}.
        /// </summary>
        public static string SpeedMBFormatStr {
            get {
                return ResourceManager.GetString("SpeedMBFormatStr", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0:###0} KB/s, {1}.
        /// </summary>
        public static string SpeedShortKBFormatStr {
            get {
                return ResourceManager.GetString("SpeedShortKBFormatStr", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0:###0} MB/s, {1}.
        /// </summary>
        public static string SpeedShortMBFormatStr {
            get {
                return ResourceManager.GetString("SpeedShortMBFormatStr", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sorry, But I could not find where steam has been installed.
        ///
        ///Please browse for it manually..
        /// </summary>
        public static string SteamFolderNotFound {
            get {
                return ResourceManager.GetString("SteamFolderNotFound", resourceCulture);
            }
        }
    }
}