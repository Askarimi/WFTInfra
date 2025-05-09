using System;
using MediatR;

namespace WFT.Infra.Application.Commands
{
    public class CreateEntityCommand : IRequest<Guid>
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public CreateEntityCommand(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
} 