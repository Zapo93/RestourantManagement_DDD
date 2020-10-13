﻿using MediatR;
using RestaurantManagement.Application.Serving.Commands.Common;
using RestaurantManagement.Application.Serving.Commands.CreateDish;
using RestaurantManagement.Domain.Serving.Factories;
using RestaurantManagement.Domain.Serving.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantManagement.Application.Serving.Commands.CreateOrder
{
    public class CreateOrderCommand: IRequest<CreateOrderOutputModel>
    {
        public int AssigneeId = default!;
        public int? TableId = null;

        public List<OrderItemInputModel> Items;

        public CreateOrderCommand() 
        {
            Items = new List<OrderItemInputModel>();
        }

        public class CreateOrderCommandHandler: IRequestHandler<CreateOrderCommand, CreateOrderOutputModel>
        {
            private readonly IOrderFactory OrderFactory;
            private readonly IOrderRepository OrderRepository;
            private readonly IDishRepository DishRepository;

            public CreateOrderCommandHandler(
                IOrderFactory orderFactory,
                IOrderRepository orderRepository,
                IDishRepository dishRepository)
            {
                OrderFactory = orderFactory;
                OrderRepository = orderRepository;
                DishRepository = dishRepository;
            }

            public async Task<CreateOrderOutputModel> Handle(CreateOrderCommand command, CancellationToken cancellationToken) 
            {
                OrderFactory
                    .WithAssignee(command.AssigneeId)
                    .WithTableId(command.TableId);

                List<OrderItem> orderItems = new List<OrderItem>();

                foreach(var item in command.Items)
                {
                    Dish dish = await DishRepository.GetDishById(item.DishId,cancellationToken);
                    orderItems.Add(new OrderItem(dish,item.Note));
                }

                OrderFactory.WithItems(orderItems);

                Order newOrder = OrderFactory.Build();
                await OrderRepository.Save(newOrder, cancellationToken);

                return new CreateOrderOutputModel(newOrder.Id);
            }
        }
    } 
}
