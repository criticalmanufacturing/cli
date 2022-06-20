// #region Import statements

/** Core */
import { PackageMetadata, Framework } from 'cmf.core/src/core'
import { MenuItem } from 'cmf.core/src/domain/metadata/menu'

/** i18n */
import i18n from './i18n/main.default'

declare var SystemJS: { import: any }

// #endregion

function applyConfig (packageName: string) {
  const config: PackageMetadata = {
    version: '',
    name: `${packageName}`,
    components: [
      // Below this line all components are attached automatically during build
      // inject:components
      // endinject:components
    ],
    directives: [
      // Below this line all directives are attached automatically during build
      // inject:directives
      // endinject:directives
    ],
    pipes: [
      // Below this line all pipes are attached automatically during build
      // inject:pipes
      // endinject:pipes
    ],
    i18n: [
      // Below this line all i18n are attached automatically during build
      // inject:i18n
      // endinject:i18n
    ],
    widgets: [
      // Below this line all widgets are attached automatically during build
      // inject:widgets
      // endinject:widgets
    ],
    dataSources: [
      // Below this line all dataSources are attached automatically during build
      // inject:dataSources
      // endinject:dataSources
    ],
    converters: [
      // Below this line all converters are attached automatically during build
      // inject:converters
      // endinject:converters
    ],
    metadataLoadedHandler: () => {
      // Place here the specific module loader configs to load the dependencies of this package
    },
    flex: {
      actionButtonGroups: [],
      actionButtons: [],
      actions: [],
      menuGroups: [
        {
          position: 2000,
          id: 'Shell.<%= $CLI_PARAM_DFPackageName %>',
          iconClass: 'icon-docs-st-lg-userguide',
          route: '<%= $CLI_PARAM_DFPackageName %>',
          itemsGenerator: class MenuGen {
            public items (framework: Framework): Promise<MenuItem[]> {
              return SystemJS.import('./node_modules/<%= $CLI_PARAM_CustomPackageName %>/assets/__generatedMenuItems.json').then((jsonContent) => {
                return jsonContent
              })
            }
          }.prototype,
          title: '<%= $CLI_PARAM_Tenant %>'
        }],
      menuItems: [
        {
          id: '<%= $CLI_PARAM_DFPackageName %>.index',
          menuGroupId: 'Shell.<%= $CLI_PARAM_DFPackageName %>',
          title: 'Index',
          actionId: ''
        },
        {
          id: '<%= $CLI_PARAM_DFPackageName %>.techspec',
          menuGroupId: 'Shell.<%= $CLI_PARAM_DFPackageName %>',
          title: 'Technical Specification',
          actionId: ''
        },
        {
          id: '<%= $CLI_PARAM_DFPackageName %>.userguide',
          menuGroupId: 'Shell.<%= $CLI_PARAM_DFPackageName %>',
          title: 'User Guide',
          actionId: ''
        },
        {
          id: '<%= $CLI_PARAM_DFPackageName %>.releasenotes',
          menuGroupId: 'Shell.<%= $CLI_PARAM_DFPackageName %>',
          title: 'Release Notes',
          actionId: ''
        },
        {
          id: '<%= $CLI_PARAM_DFPackageName %>.faq',
          menuGroupId: 'Shell.<%= $CLI_PARAM_DFPackageName %>',
          title: 'FAQ',
          actionId: ''
        }
      ],
      entityTypes: [],
      routes: [{
        routeConfig: []
      }]
    }
  }
  return config
}

export default applyConfig
