using AutoMapper;
using AutoMapper.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RPK.Infra.Application.Contracts.Interfaces;
using RPK.Infra.Application.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RPK.Infra.Application.Services
{
    public class EnhancedMapperService : IMapperService
    {
        private readonly IMapper _mapper;
        private readonly MapperConfiguration _config;

        public EnhancedMapperService(IServiceProvider serviceProvider)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name.StartsWith("RPK.Infra"))
                .ToArray();

            var config = new MapperConfigurationExpression();
            config.AddMaps(assemblies);

            // Add global configuration
            config.CreateMissingTypeMaps = true;
            config.ValidateInlineMaps = false;

            _config = new MapperConfiguration(config);
            _config.AssertConfigurationIsValid();
            _config.Compile();

            _mapper = new Mapper(_config, serviceProvider.GetService);
        }

        public TDestination Map<TSource, TDestination>(TSource source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), "Source object cannot be null");

            try
            {
                return _mapper.Map<TSource, TDestination>(source);
            }
            catch (AutoMapperMappingException ex)
            {
                throw new MappingException($"Error mapping from {typeof(TSource).Name} to {typeof(TDestination).Name}", ex);
            }
        }

        public IEnumerable<TDestination> MapList<TSource, TDestination>(IEnumerable<TSource> source)
        {
            if (source == null)
                return Enumerable.Empty<TDestination>();

            try
            {
                return _mapper.Map<IEnumerable<TSource>, IEnumerable<TDestination>>(source);
            }
            catch (AutoMapperMappingException ex)
            {
                throw new MappingException($"Error mapping collection from {typeof(TSource).Name} to {typeof(TDestination).Name}", ex);
            }
        }
    }

    public class MappingException : Exception
    {
        public MappingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
} 