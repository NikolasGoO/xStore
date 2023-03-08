using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xStore.Core.DomainObjects;
using xStore.Core.Enums;

namespace xStore.Domain.Entities
{
    public class Order : Entity
    {
        protected Order() { }
        public Guid ClientId { get; private set; }
        public decimal TotalValue { get; private set; }
        public Guid? VoucherId { get; private set; }
        public bool VoucherUsed { get; private set; }
        public decimal Discount { get; private set; }
        public int Code { get; private set; }
        public OrderStatus OrderStatus { get; private set; }
        private readonly List<OrderItem> _orderItems;
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems;

        //EF Rel.
        public Address Address { get; private set; }
        public Voucher Voucher { get; private set; }

        public Order(Guid clientId, decimal totalValue, List<OrderItem> orderItems, bool voucherUsed, decimal discount = 0, Guid? voucherId = null)
        {
            ClientId = clientId;
            TotalValue = totalValue;
            _orderItems = orderItems;
            VoucherUsed = voucherUsed;
            Discount = discount;
            VoucherId = voucherId;
        }

        public void AuthorizedOrder()
        {
            OrderStatus = OrderStatus.Autorizado;
        }

        public void CanceledOrder()
        {
            OrderStatus = OrderStatus.Cancelado;
        }

        public void FinalizeOrder()
        {
            OrderStatus = OrderStatus.Pago;
        }

        public void SetVoucher(Voucher voucher)
        {
            VoucherUsed = true;
            Voucher = voucher;
            VoucherId = voucher.Id;
        }

        public void CalculateOrderValue()
        {
            TotalValue = OrderItems.Sum(p => p.CalculeValue());
            CalculateValueTotalDiscount();
        }

        public void CalculateValueTotalDiscount()
        {
            if (!VoucherUsed) return;
            decimal discount = 0;
            var value = TotalValue;

            if (Voucher.DiscountType == DiscountTypeVoucher.Porcentagem)
            {
                if (Voucher.Percentage.HasValue)
                {
                    discount = (value * Voucher.Percentage.Value) / 100;
                    value -= discount;
                }
            }
            else
            {
                if(Voucher.DiscountValue.HasValue)
                {
                    discount = Voucher.DiscountValue.Value;
                    value -= discount;
                }
            }

            if(value < 0) 
            {
                TotalValue = 0;
            }
            else
            {
                TotalValue = value;
            }

            Discount = discount;
        }
    }
}
