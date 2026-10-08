using ERP.Models.Inventory.InvDepartmentRequisition;
using ERP.Models.Inventory.InvDepartmentRequisitionDetail;
using System.Collections.Generic;
using System.Web;

namespace ERP.DAL.Inventory
{
    public class InvRequisitionMovementDAL
    {
        //public static InvDepartmentRequisition2ndDAL.InvDepartmentRequisitionResponse GetFiltered(
        //    HttpRequestBase request, int? statusFilter)
        //{
        //   // return InvDepartmentRequisition2ndDAL.GetFilteredRequisitionsForMovement(request, statusFilter);
        //}

        public static InvDepartmentRequisitionInfo GetById(string id)
        {
            return InvDepartmentRequisitionDAL.GetRequisitionByID(id);
        }

        public static void Forward(string id, string user, string remarks)
        {
            InvDepartmentRequisition2ndDAL.ForwardRequisition(id, user, remarks);
        }

        public static void Approve(string id, string user, string remarks,
            List<InvDepartmentRequisitionDetailInfo> lines)
        {
           // InvDepartmentRequisition2ndDAL.ApproveRequisition(id, user, remarks, lines);
        }

        public static void Reject(string id, string user, string remarks)
        {
            InvDepartmentRequisition2ndDAL.RejectRequisition(id, user, remarks);
        }
    }
}