using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nop.Plugin.Misc.OneS.Models
{
    public class OrderOneS
    {
        public OrderOneS() 
        {
            OrderItems=new Collection<OrderItem>();
        }
        public DateTime Date { get; set; }
        public int Id { get; set; }
        public DateTime CreationDate { get; set; }
        public string CostomerName { get; set; }
        public string CostomerPhone { get; set; }
        public string CostomerEmail { get; set; }
        public bool CostpmerIsLegal { get; set; }
        public string CostomerNote { get; set; }
        public string PaymentId { get; set; }
        public string Payment { get; set; }
        public string Advance { get; set; }
        public string IsPaid { get; set; }
        public string DeliveryId { get; set; }
        public string Delivery { get; set; }
        public string DeliveryCity { get; set; }
        public string DeliveryCityId { get; set; }
        public string DeliveryDate { get; set; }
        public string DeliveryAdress { get; set; }
        public string ResponsibleUser { get; set; }
        public string ResponsibleUserId { get; set; }
        public string StateId { get; set; }
        public string State { get; set; }
        public DateTime StateDate { get; set; }
        public bool IsConditionalReserve { get; set; }
        public bool CostomerIsLegal { get; set; }
        public string CancelReason { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } 
        
    }

    public class OrderItem
    {
        public OrderItem()
        {
            
        }
        public int Id { get; set; }
        public string StateId { get; set; }
        public string ProductMarkaId { get; set; }
        public string ProductId { get; set; }
        public string Product { get; set; }
        public string ModelId { get; set; }
        public string Model { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string UnitTitle { get; set; }
        public decimal Sum { get; set; }
        public string StorageId { get; set; }
    }
}
