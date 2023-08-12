using System.Collections.Generic;
using Z.Model.System;

namespace Z.Model.System.Dto
{
    public class SysdictDataDto
    {
        public string DictType { get; set; }
        public string ColumnName { get; set; }
        public List<SysDictData> List { get; set; }
    }
}
