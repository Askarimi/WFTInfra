using AutoMapper;
using System;

namespace WFT.Infra.Application.Mapping
{
    public abstract class BaseProfile : Profile
    {
        protected BaseProfile()
        {
            ConfigureMappings();
        }

        protected abstract void ConfigureMappings();
    }
} 