import {
    Injectable
} from '@angular/core';

import {
    RouteConfig,
    PackageMetadata,
    Action,
    MenuGroup,
    MenuItem,
    ActionButton,
    ActionButtonGroup,
    EntityTypeMetadata,
    PackageInfo
} from 'cmf-core';


@Injectable()
export class DocsArea<%= $CLI_PARAM_DFPackageNamePascalCase %>MetadataService extends PackageMetadata {

    /**
     * Package Info
     */
    public override get packageInfo(): PackageInfo {
        return {
            name: '<%= $CLI_PARAM_CustomPackageName %>',
            loader: () => import(
                /* webpackExports: [] */
                '<%= $CLI_PARAM_CustomPackageName %>'),
            converters: [],
            widgets: [],
            dataSources: [],
            components: []
        };
    }

    /**
     * Action Button Groups
     */
    public override get actionButtonGroups(): ActionButtonGroup[] {
        return [];
    }

    /**
     * Action Buttons
     */
    public override get actionButtons(): ActionButton[] {
        return [];
    }

    /**
     * Actions
     */
    public override get actions(): Action[] {
        return [];
    }

    /**
     * Menu Groups
     */
    public override get menuGroups(): MenuGroup[] {
        return [
            {
                position: 2000,
                id: 'Shell.<%= $CLI_PARAM_DFPackageName %>',
                iconClass: 'icon-docs-st-lg-userguide',
                route: '<%= $CLI_PARAM_DFPackageName %>',
                itemsGenerator: class MenuGen {
                    public items (): Promise<MenuItem[]> {
                      return fetch('./assets/<%= $CLI_PARAM_CustomPackageName %>/__generatedMenuItems.json').then((response) => {
                        return response.json();
                      });
                    }
                }.prototype,
                title: 'DevOps'
              }
        ];
    }

    /**
     * Menu Items
     */
    public override get menuItems(): MenuItem[] {
        return [
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
        ];
    }

    /**
     * Entity Types
     */
    public override get entityTypes(): EntityTypeMetadata[] {
        return [];
    }

    /**
     * Routes
     */
    public override get routes(): RouteConfig[] {
        return [];
    }
}
