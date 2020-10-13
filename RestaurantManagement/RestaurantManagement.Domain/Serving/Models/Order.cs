﻿using RestaurantManagement.Domain.Common;
using RestaurantManagement.Domain.Serving.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantManagement.Domain.Serving.Models
{
    public class Order: Entity<int>, IAggregateRoot
    {
        internal Order(int assigneeId, int? tableId = null)
        {
            items = new List<OrderItem>();
            kitchenRequestIds = new HashSet<string>();
            TableId = tableId;
            DateCreated = DateTime.UtcNow;
            AssigneeId = assigneeId;
            Open = true;
        }

        private List<OrderItem> items;
        public IReadOnlyCollection<OrderItem> Items => items.ToList().AsReadOnly();

        private HashSet<string> kitchenRequestIds;
        public IReadOnlyCollection<string> KitchenRequestIds => kitchenRequestIds.ToList().AsReadOnly();

        public int? TableId { get; private set; }

        public DateTime DateCreated { get; private set; }
        public int AssigneeId { get; private set; }

        //TODO check if this is initialized correctly when taken from persisense
        public bool Open { get; private set; }

        public void Close() 
        {
            Open = false;
        }

        public void AddItems(IEnumerable<OrderItem> newItems)
        {
            if (Open)
            {
                items.AddRange(newItems);
                string requestId = GenerateKitchenRequestId();
                AddKitchenRequestById(requestId);
            }
            else
            {
                throw new OrderClosedException("Can not add items on closed order!");
            }
        }

        private string GenerateKitchenRequestId() 
        {
            return new Guid().ToString().Substring(0, 8);
        }

        private void AddKitchenRequestById(string kitchenRequestId)
        {
            kitchenRequestIds.Add(kitchenRequestId);
        }

        public Money TotalPrice { get {return GetTotalPrice(); } }

        public Money GetTotalPrice() 
        {
            Money totalPrice = new Money(0);
            foreach (OrderItem item in items) 
            {
                totalPrice.Add(item.Dish.Price);
            }

            return totalPrice;
        }
    }
}
