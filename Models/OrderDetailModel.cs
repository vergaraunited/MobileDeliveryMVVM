using System.ComponentModel;
using MobileDeliveryGeneral.Data;
using static MobileDeliveryGeneral.Definitions.MsgTypes;

namespace MobileDeliveryMVVM.Models
{
    public class OrderDetailModel : INotifyPropertyChanged
    {
        public int LineNumber { get; set; }
        public long ORD_NO { get; set; }
        public string MDL_NO { get; set; }
        public short OPT_NUM { get; set; }
        public string DESC { get; set; }
        public string CLR { get; set; }
        public short MDL_CNT { get; set; }
        public short WIN_CNT { get; set; }
        public short PAT_POS { get; set; }

        public bool IsSelected { get; set; }

        public OrderDetailModel(orderDetails mst)
        {
            ORD_NO = mst.ORD_NO;
            MDL_NO = mst.MDL_NO;
            OPT_NUM = mst.OPT_NUM;
            DESC = mst.DESC;
            CLR = mst.CLR;
            PAT_POS = mst.PAT_POS;
            MDL_CNT = mst.MDL_CNT;
        }

        public OrderDetailModel(OrderDetailsData odd)
        {
            ORD_NO = odd.ORD_NO;
            MDL_NO = odd.MDL_NO;
            OPT_NUM = odd.OPT_NUM;
            DESC = odd.DESC;
            CLR = odd.CLR;
            PAT_POS = odd.PAT_POS;
            MDL_CNT = odd.MDL_CNT;
        }

        public OrderDetailModel(orderMaster mst)
        {
           // ORD_NO = mst.ORD_NO;
            //DLR_NO = mst.DLR_NO;
            //OPT_NUM = BitConverter.ToInt16(mst.OPT_NUM, 0);
            //DESC = BitConverter.ToString(mst.DESC, 0, fldsz_DESCOrd);
            //CLR = BitConverter.ToString(mst.CLR, 0, fldsz_CLR);
            //WIDTH = BitConverter.ToInt64(mst.WIDTH, 0);
            //HEIGHT = BitConverter.ToInt64(mst.HEIGHT, 0);
            //MDL_CNT = BitConverter.ToInt16(mst.MDL_CNT, 0);
            //WIN_CNT = BitConverter.ToInt16(mst.WIN_CNT, 0);
            //RTE_CDE = mst.RTE_CDE;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        //z List<Item> itList = new List<Item>();

    }
}
