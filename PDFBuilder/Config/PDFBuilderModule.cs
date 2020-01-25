using System;
using System.Reflection;
using Autofac;
using Microsoft.Extensions.Configuration;
using Module = Autofac.Module;

namespace PDFBuilder.Config
{
    public class PDFBuilderModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public PDFBuilderModule()
        {}

        /// <summary>
        /// Constructor to pass configuration object in
        /// </summary>
        /// <param name="configuration">Configuration object built from appsettings.json file</param>
        public PDFBuilderModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private readonly IConfiguration _configuration;

        ///<inheritdoc />
        protected override void Load(ContainerBuilder builder)
        {
            
        }
    }
}
