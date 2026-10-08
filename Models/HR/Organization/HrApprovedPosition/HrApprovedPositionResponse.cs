using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.HR.Organization.HrApprovedPosition
{
    public class HrApprovedPositionResponse : WebResponse
    {
        public HrApprovedPositionResponse()
        {
            ResponseTS = DateTime.Now;
            HrApprovedPositions = new List<HrApprovedPositionInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public HrApprovedPositionInfo HrApprovedPosition;
        public List<HrApprovedPositionInfo> HrApprovedPositions;
    }
}