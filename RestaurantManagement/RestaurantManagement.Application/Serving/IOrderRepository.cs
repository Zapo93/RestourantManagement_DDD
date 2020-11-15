﻿using RestaurantManagement.Common.Application.Contracts;
using RestaurantManagement.Common.Domain;
using RestaurantManagement.Domain.Serving.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantManagement.Application.Serving
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrders(Specification<Order> orderSpec, CancellationToken cancellationToken);
        Task<Order> GetOrderById(int orderId, CancellationToken cancellationToken);
        Task<Order> GetOrderByRequestId(string creatorReferenceId, CancellationToken cancellationToken);
    }
}
