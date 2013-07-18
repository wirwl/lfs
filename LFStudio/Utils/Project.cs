using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using lf2dat;

namespace LFStudio.Utils
{
    public static class Project
    {
        public static cProject GetProject(int index)
        {
            if (index == -1) return null;
            return G.mainWindow.lProjects[index];
        }
        public static string GetNameProject(cProject cp)
        {
            if (cp == null) return "no project";
            return (cp.files[0] as ArrayList)[0] as string;
        }
        public static int GetProjectIndex(List<cProject> lcp , string name)
        {
            for (int i=0;i<lcp.Count;i++)
            {
                if (name == GetNameProject(lcp[i])) return i;
            }
            return -1;
        }
        public static string GetProjectPassword(List<cProject> lp, int np)
        {
            if (np == -1) return G.AppSettings.StandardPassword;
            return lp[np].pass;
        }
    }
}
