using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LFStudio;

namespace LFStudio
{   
    public interface IPlugin
    {
        string Name { get; }
        int Build { get; }
        string Author { get; }
        string Email { get; }
        string Url { get; }
        string Comment { get; }      
        void NormalExecute();
        void SilentExecute(Host AppObjects);
        bool MultipleInstanceAllow { get; }
        bool ShowInPluginMenu { get; }
    }

}
