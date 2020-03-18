using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UMDGeneral.Data;
using UMDGeneral.ExtMethods;
using static UMDGeneral.Definitions.MsgTypes;

namespace MobileDeliveryMVVM.Models
{
    public class OrderDetailModel : INotifyPropertyChanged
    {
        public int LineNumber { get; set; }
        public long ORD_NO { get; set; }
        public short MDL_NO { get; set; }
        public long DLR_NO { get; set; }
        public short OPT_NUM { get; set; }
        public string DESC { get; set; }
        public string CLR { get; set; }
        public long WIDTH { get; set; }
        public long HEIGHT { get; set; }
        public short MDL_CNT { get; set; }
        public short WIN_CNT { get; set; }
        public string RTE_CDE { get; set; }

        public bool IsSelected { get; set; }

        public OrderDetailModel(orderDetails mst)
        {
            ORD_NO = BitConverter.ToInt32(mst.ORD_NO, 0);
            MDL_NO = BitConverter.ToInt16(mst.MDL_NO, 0);
            OPT_NUM = BitConverter.ToInt16(mst.OPT_NUM, 0);
            DESC = mst.DESC.UMToString(fldsz_DESCOrd);
            CLR = mst.CLR.UMToString(fldsz_CLR);
            WIDTH = BitConverter.ToInt32(mst.WIDTH, 0);
            HEIGHT = BitConverter.ToInt32(mst.HEIGHT, 0);
            MDL_CNT = BitConverter.ToInt16(mst.MDL_CNT, 0);
            // WIN_CNT = BitConverter.ToInt16(mst.LINK_WIN_CNT, 0);
            // RTE_CDE = Convert.ToString(mst.TRUCK);
        }

        public OrderDetailModel(OrderDetailsData odd)
        {
            ORD_NO = odd.ORD_NO;
            MDL_NO = odd.MDL_NO;
            OPT_NUM = odd.OPT_NUM;
            DESC = odd.DESC;
            CLR = odd.CLR;
            WIDTH = 0;
            HEIGHT = 0;
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
