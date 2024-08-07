﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Phx.Inject.Generator 0.6.0.0.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable
namespace Phx.Inject.Tests {
    internal partial class GeneratedMultiBindInjector : Phx.Inject.Tests.IMultiBindInjector {
        internal record SpecContainerCollection (
                Phx.Inject.Tests.GeneratedMultiBindInjector_MultiBindSpecification GeneratedMultiBindInjector_MultiBindSpecification
        ) {
            internal SpecContainerCollection CreateNewFrame() {
                return new SpecContainerCollection(
                        GeneratedMultiBindInjector_MultiBindSpecification.CreateNewFrame());
            }
        }

        private readonly SpecContainerCollection specContainers;

        public GeneratedMultiBindInjector() {
            specContainers = new SpecContainerCollection(
                    GeneratedMultiBindInjector_MultiBindSpecification: new Phx.Inject.Tests.GeneratedMultiBindInjector_MultiBindSpecification());
        }

        public System.Collections.Generic.List<Phx.Inject.Tests.Data.Model.ILeaf> GetLeafList() {
            return Phx.Inject.InjectionUtil.Combine<Phx.Inject.Tests.Data.Model.ILeaf> (
                specContainers.GeneratedMultiBindInjector_MultiBindSpecification.GetListLeaf1(specContainers),
                specContainers.GeneratedMultiBindInjector_MultiBindSpecification.GetListLeaf2(specContainers)
            );
        }

        public System.Collections.Generic.HashSet<Phx.Inject.Tests.Data.Model.ILeaf> GetLeafSet() {
            return Phx.Inject.InjectionUtil.Combine<Phx.Inject.Tests.Data.Model.ILeaf> (
                specContainers.GeneratedMultiBindInjector_MultiBindSpecification.GetSetLeaf1(specContainers),
                specContainers.GeneratedMultiBindInjector_MultiBindSpecification.GetSetLeaf2(specContainers)
            );
        }

        public System.Collections.Generic.Dictionary<System.String,Phx.Inject.Tests.Data.Model.ILeaf> GetLeafDict() {
            return Phx.Inject.InjectionUtil.Combine<System.String, Phx.Inject.Tests.Data.Model.ILeaf> (
                specContainers.GeneratedMultiBindInjector_MultiBindSpecification.GetDictLeaf1(specContainers),
                specContainers.GeneratedMultiBindInjector_MultiBindSpecification.GetDictLeaf2(specContainers)
            );
        }

        public Phx.Inject.Factory<System.Collections.Generic.List<Phx.Inject.Tests.Data.Model.ILeaf>> GetLeafListRuntimeFactory() {
            return new Phx.Inject.Factory<System.Collections.Generic.List<Phx.Inject.Tests.Data.Model.ILeaf>>(() => Phx.Inject.InjectionUtil.Combine<Phx.Inject.Tests.Data.Model.ILeaf> (
                specContainers.GeneratedMultiBindInjector_MultiBindSpecification.GetListLeaf1(specContainers),
                specContainers.GeneratedMultiBindInjector_MultiBindSpecification.GetListLeaf2(specContainers)
            ));
        }

        public Phx.Inject.Factory<System.Collections.Generic.HashSet<Phx.Inject.Tests.Data.Model.ILeaf>> GetLeafSetRuntimeFactory() {
            return new Phx.Inject.Factory<System.Collections.Generic.HashSet<Phx.Inject.Tests.Data.Model.ILeaf>>(() => Phx.Inject.InjectionUtil.Combine<Phx.Inject.Tests.Data.Model.ILeaf> (
                specContainers.GeneratedMultiBindInjector_MultiBindSpecification.GetSetLeaf1(specContainers),
                specContainers.GeneratedMultiBindInjector_MultiBindSpecification.GetSetLeaf2(specContainers)
            ));
        }

        public Phx.Inject.Factory<System.Collections.Generic.Dictionary<System.String,Phx.Inject.Tests.Data.Model.ILeaf>> GetLeafDictRuntimeFactory() {
            return new Phx.Inject.Factory<System.Collections.Generic.Dictionary<System.String,Phx.Inject.Tests.Data.Model.ILeaf>>(() => Phx.Inject.InjectionUtil.Combine<System.String, Phx.Inject.Tests.Data.Model.ILeaf> (
                specContainers.GeneratedMultiBindInjector_MultiBindSpecification.GetDictLeaf1(specContainers),
                specContainers.GeneratedMultiBindInjector_MultiBindSpecification.GetDictLeaf2(specContainers)
            ));
        }
    }
}
#nullable restore
