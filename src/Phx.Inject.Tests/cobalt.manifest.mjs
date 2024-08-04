/* 
 * cobalt.manifest.mjs
 * 
 * Copyright (c) 2024 Star Cruise Studios LLC. All rights reserved.
 * Licensed under the Apache License 2.0.
 * See https://www.apache.org/licenses/LICENSE-2.0 for full license information.
 */
import { versionSet } from '../../cobalt.manifest.mjs';

export default {
    properties: {
        cobaltVersion: '1.0.x',
        root: '../..',
        versionSet: versionSet,
    },
    project: async (cobalt, context) => {
        cobalt.plugins.add(
            context.versions.cobalt.plugin.dotnet,
            context.versions.plugins.phx.csprojcompany,
            context.versions.plugins.phx.cstest,
        )
        cobalt.stash.add(
            { artifact: context.versions.stash.phx.nlog.test, path: '.' }
        )
    },
    configure: async (cobalt, context) => {
        cobalt.config.set('project', {
            artifact: { artifact: 'Phx.Inject.Tests', version: '1.0.0' },
            props: {
                ImplicitUsings: 'enable',
                RootNamespace: '',
                EmitCompilerGeneratedFiles: 'true',
                CompilerGeneratedFilesOutputPath: '$(BaseIntermediateOutputPath)\\GeneratedFiles',
            }
        });
        cobalt.dependencies.add(
            context.versions.microsoft.codeanalysis.csharp.sourcegenerators.testing.nunit,
            context.versions.microsoft.codeanalysis.csharp.csharp,
            context.versions.microsoft.codeanalysis.csharp.workspaces,
        )
    }
}
