using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace LFStudio.Windows
{
    internal class OutputReader
    {
        private Process _process;
        private string _output;
        private string _error;
        /// <summary>
        /// Конструктор, принимает хэндл экзешника
        /// из которого callback методы в последствии будут читать...
        /// </summary>
        /// <param name="process">Экземпляр хэндла</param>
        public OutputReader(Process process)
        {
            _process = process;
        }

        /// <summary>
        /// Callback, читающий из стандартного вывода
        /// </summary>
        public void OutReader()
        {
            _output = _process.StandardOutput.ReadToEnd();
        }
        /// <summary>
        /// Callback читающий из вывода для ошибок
        /// </summary>
        public void ErrorReader()
        {
            _error = _process
            .StandardError.ReadToEnd();
        }
        /// <summary>
        /// Свойство возвращающее полученное из стандартного вывода
        /// </summary>
        public string Output
        {
            get { return _output; }
        }
        /// <summary>
        /// Свойство возвращающее полученное из вывода для ошибок
        /// </summary>
        public string Error
        {
            get { return _error; }
        }
    }
}