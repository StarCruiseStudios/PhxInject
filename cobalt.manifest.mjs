/* 
 * cobalt.manifest.mjs
 * 
 * Copyright (c) 2024 Star Cruise Studios LLC. All rights reserved.
 * Licensed under the Apache License 2.0.
 * See https://www.apache.org/licenses/LICENSE-2.0 for full license information.
 */

export const versionSet = {
  name: 'starcruisestudios.phx',
  version: '0.0.x'
};

export default {
  properties: {
    cobaltVersion: '1.0.x',
    versionSet: versionSet,
    multiProject: true,
  },
  configure: async (cobalt, context) => {
    cobalt.config.multiProject = {
      projectDirs: await context.io.findDirectoriesContaining('.', 'cobalt.manifest.mjs', ['obj', 'build', 'test'])
    };
  }
}
