// -----------------------------------------------------------------------------
// <copyright file="ChildInjectorAttributeTransformer.cs" company="Star Cruise Studios LLC">
//     Copyright (c) 2026 Star Cruise Studios LLC. All rights reserved.
//     Licensed under the Apache License, Version 2.0.
//     See http://www.apache.org/licenses/LICENSE-2.0 for full license information.
// </copyright>
// -----------------------------------------------------------------------------

#region

using Microsoft.CodeAnalysis;
using Phx.Inject.Generator.Incremental.Diagnostics;
using Phx.Inject.Generator.Incremental.Stage1.Metadata.Model.Attributes;

#endregion

namespace Phx.Inject.Generator.Incremental.Stage1.Metadata.Pipeline.Attributes;

/// <summary>
///     Transforms ChildInjector attribute data into metadata.
/// </summary>
/// <remarks>
///     <para>Purpose - Scoped Container Hierarchy:</para>
///     <para>
///     [ChildInjector] marks specifications that create child scopes within a parent injector hierarchy.
///     Child injectors inherit parent dependencies but can override them with scoped instances.
///     This enables request-scoped or transaction-scoped dependencies that differ from application-level
///     singleton dependencies.
///     </para>
///     
///     <para>User Code Pattern - Parent and Child Injectors:</para>
///     <code>
///     [Specification, Injector]
///     public interface IApplicationScope {
///         [Factory] IDatabase CreateDatabase();
///         
///         IRequestScope CreateRequestScope();  // Factory for child
///     }
///     
///     [Specification, ChildInjector]
///     public interface IRequestScope {
///         [Factory] IRequestContext CreateRequestContext();
///         
///         // Inherits IDatabase from parent IApplicationScope
///         void HandleRequest(IDatabase db, IRequestContext context);
///     }
///     </code>
///     <para>
///     When CreateRequestScope() is called, generator creates a new IRequestScope implementation
///     that has access to parent's IDatabase but manages its own IRequestContext lifecycle.
///     </para>
///     
///     <para>Why ChildInjector Needs Special Handling - Parent Context Propagation:</para>
///     <para>
///     ChildInjector specifications require special handling because:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Generated child implementation must accept parent injector in constructor to access inherited dependencies
///             </description>
///         </item>
///         <item>
///             <description>
///             Dependency resolution walks up parent chain (check child first, then parent, then grandparent)
///             </description>
///         </item>
///         <item>
///             <description>
///             Child can override parent dependencies with different implementations or scopes
///             </description>
///         </item>
///         <item>
///             <description>
///             Lifecycle management differs: child-scoped instances dispose when child disposes, not parent
///             </description>
///         </item>
///     </list>
///     
///     <para>No Arguments - Hierarchical Structure Implied:</para>
///     <para>
///     ChildInjector attribute has no configuration arguments because:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Parent-child relationship is determined by factory method return types (IRequestScope CreateRequestScope())
///             </description>
///         </item>
///         <item>
///             <description>
///             Inheritance behavior is standard (search child first, then parent)
///             </description>
///         </item>
///         <item>
///             <description>
///             No need to configure which parent (factory method determines this)
///             </description>
///         </item>
///     </list>
///     
///     <para>ChildInjector vs Injector - Different Generated Patterns:</para>
///     <para>
///     The presence of [ChildInjector] vs [Injector] changes code generation:
///     </para>
///     <list type="bullet">
///         <item>
///             <term>Injector (root):</term>
///             <description>
///             Generated class is self-contained, constructor accepts external dependencies only.
///             No parent injector field. Example: ApplicationScopeImpl()
///             </description>
///         </item>
///         <item>
///             <term>ChildInjector:</term>
///             <description>
///             Generated class constructor accepts parent injector. Has parent field for dependency lookup.
///             Example: RequestScopeImpl(IApplicationScope parent)
///             </description>
///         </item>
///     </list>
///     
///     <para>Validation Constraints - Enforced by Later Stages:</para>
///     <para>
///     Transformer doesn't validate hierarchy correctness. Later validation ensures:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///             Child injector is returned by exactly one parent factory method
///             </description>
///         </item>
///         <item>
///             <description>
///             No circular parent-child relationships (A creates B, B creates A)
///             </description>
///         </item>
///         <item>
///             <description>
///             Child doesn't request dependencies that aren't available in parent chain
///             </description>
///         </item>
///         <item>
///             <description>
///             [Injector] and [ChildInjector] aren't used simultaneously on same specification
///             </description>
///         </item>
///     </list>
///     
///     <para>Common Errors Prevented:</para>
///     <list type="bullet">
///         <item>
///             <term>Missing parent factory:</term>
///             <description>
///             Child injector without any parent factory method is orphaned. Validator ensures
///             every child has exactly one parent factory.
///             </description>
///         </item>
///         <item>
///             <term>Multiple parents:</term>
///             <description>
///             Child injector returned by multiple factories is ambiguous (which parent owns it?).
///             Validator requires single parent factory.
///             </description>
///         </item>
///         <item>
///             <term>Circular hierarchy:</term>
///             <description>
///             Parent creates child, child creates original parent causes infinite recursion.
///             Validator detects cycles in injector hierarchy.
///             </description>
///         </item>
///         <item>
///             <term>Using both [Injector] and [ChildInjector]:</term>
///             <description>
///             Mutually exclusive attributes. Specification is either root or child, not both.
///             </description>
///         </item>
///     </list>
/// </remarks>
internal sealed class ChildInjectorAttributeTransformer(
    IAttributeMetadataTransformer attributeMetadataTransformer
) : IAttributeTransformer<ChildInjectorAttributeMetadata> {
    /// <summary>
    ///     Gets the singleton instance.
    /// </summary>
    public static ChildInjectorAttributeTransformer Instance { get; } = new(
        AttributeMetadataTransformer.Instance
    );

    /// <inheritdoc />
    public bool HasAttribute(ISymbol targetSymbol) {
        return attributeMetadataTransformer.HasAttribute(targetSymbol, ChildInjectorAttributeMetadata.AttributeClassName);
    }

    /// <inheritdoc />
    public IResult<ChildInjectorAttributeMetadata> Transform(ISymbol targetSymbol) {
        var (attributeData, attributeMetadata) = attributeMetadataTransformer.ExpectSingleAttribute(
            targetSymbol,
            ChildInjectorAttributeMetadata.AttributeClassName
        );
        
        return new ChildInjectorAttributeMetadata(attributeMetadata).ToOkResult();
    }
}
