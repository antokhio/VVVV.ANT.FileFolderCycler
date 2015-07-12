#region usings
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Collections.Generic;
using System.Linq;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "TestFileFold", Category = "Value", Help = "Basic template with one value in/out", Tags = "")]
    #endregion PluginInfo
    public class ValueTestFileFoldNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("File Name", StringType = StringType.Filename, DefaultString = "")]
        IDiffSpread<string> FInput;

  /*      [Input("Enable Subdir")]
        IDiffSpread<bool> FSubDir;*/
        [Input("Filter", DefaultString = "*.*")]
        IDiffSpread<string> FFilter;

        [Input("Enable Subdir")]
        IDiffSpread<bool> FSubDir;

        [Input("Bus Clear", IsBang = true)]
        ISpread<bool> FInClearBus;

        [Input("Bus Next", IsBang = true)]
        ISpread<bool> FBusNext;

        [Input("Bux Prev", IsBang = true)]
        ISpread<bool> FBusPrev;

        [Input("Bus Rnd", IsBang = true)]
        ISpread<bool> FBusRnd;

        [Output("Bus Output")]
        ISpread<string> FOutput;

        [Output("File Count")]
        ISpread<int> FOutFileCount;



        bool valid = false;
        string root = "";
        string[] files;
        int offset = 0;

        int elemcnt = 0;

        bool multipage = false;

        List<string>[] selfilesfolder;
        string[] selfiles;
        int[] selfilesidx;

        string presel;

        

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            bool rewrite = false;
            bool reloaded = false;

            if (FInput.IsChanged || FFilter.IsChanged)
            {
                root = Path.GetDirectoryName(FInput[0]);
                if (Directory.Exists(root))
                {
                    valid = true;
                    SearchOption opt = SearchOption.TopDirectoryOnly;
                    if (FSubDir[0]) { opt = SearchOption.AllDirectories; }

                    List<string> lf = new List<string>(GetFiles(root, FFilter[0], opt));
                    lf.Sort();
                    files = lf.ToArray();
                }
				else
				{
					valid= false;
					files = new string[0];
				}
				rewrite = true;
				reloaded = true;
				FOutFileCount[0] = files.Length;
			}
            if (valid)
			{
                for (int i = 0; i < selfiles.Length; i++)
                {
                    if (FBusNext[i] && selfilesidx[i] != -1)
                    {
                        selfilesidx[i] = selfilesidx[i] + 1;
                        if (selfilesidx[i] == selfilesfolder[i].Count) { selfilesidx[i] = 0; }
                        selfiles[i] = selfilesfolder[i][selfilesidx[i]];
                    }
                    if (FBusPrev[i] && selfilesidx[i] != -1)
                    {
                        selfilesidx[i] = selfilesidx[i] - 1;
                        if (selfilesidx[i] == -1) { selfilesidx[i] = selfilesfolder[i].Count - 1; }
                        selfiles[i] = selfilesfolder[i][selfilesidx[i]];
                    }
                    if (FBusRnd[i] && selfilesidx[i] != -1)
                    {
                        Random r = new Random();
                        int id = r.Next(0, selfilesfolder[i].Count - 1);
                        selfiles[i] = selfilesfolder[i][id];
                    }
                    if (FInClearBus[i])
                    {
                        selfilesidx[i] = -1;
                        selfiles[i] = "";
                    }
                }
                if (rewrite)
                {
                    int cnt = files.Length;
					
					FOutput.SliceCount = cnt;
					
					for (int i = 0;i < cnt;i++)
					{
						FOutput[i] = files[VMath.Zmod((i+ offset),files.Length)];
					}
				}
			}
			else
			{
				FOutput.SliceCount = 0;				
			}

        }
			
		private static string[] GetFiles(string sourceFolder, string filters, System.IO.SearchOption searchOption);
		
    	{
        	return filters.Split('|').SelectMany(filter => System.IO.Directory.GetFiles(root, filter, searchOption)).ToArray();
    	}
	}