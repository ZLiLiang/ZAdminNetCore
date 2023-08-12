using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z.CodeGenerator.Model
{
    /// <summary>
    /// Oracle库序列
    /// </summary>
    public class OracleSeq
    {
        public string SEQUENCE_NAME { get; set; }
        public long LAST_NUMBER { get; set; }
    }
}
