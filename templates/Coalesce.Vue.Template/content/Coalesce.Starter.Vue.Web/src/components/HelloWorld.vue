<template>
  <v-container max-width="1500px">
    <v-row>
      <v-col cols="12" md="8">
        <v-card>
          <v-card-title>
            {{ msg }}
          </v-card-title>
          <v-card-text>
            Here are some next steps to get started:

            <h2 class="mt-5">1. Create a Data Model</h2>
            <p>
              Visit
              <a href="https://learn.microsoft.com/en-us/ef/core/modeling/"
                >Entity Framework's documentation</a
              >
              to learn about creating the data model for your application.
            </p>

            <h2 class="mt-5">2. Create a Migration</h2>
            To turn your data model into a database schema,
            <a
              href="https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli"
              >create a migration with Entity Framework</a
            >.

            <!--#if (!Identity) -->
            <h2 class="mt-5">3. Add Authentication</h2>
            <strong> Implement a proper authentication mechanism</strong> in
            Program.cs and remove the existing dummy authentication. If you want
            to use
            <a
              href="https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity"
            >
              ASP.NET Core Identity </a
            >, you should start over and include the Identity option when
            instantiating the Coalesce project template. Otherwise,
            <a
              href="https://learn.microsoft.com/en-us/aspnet/core/security/how-to-choose-identity-solution"
            >
              read more about authentication in ASP.NET Core </a
            >.
            <br />
            <br />
            This template authenticates all users as a dummy user to facilitate
            rapid initial development. Since all Coalesce endpoints require
            authentication by default, you would otherwise have to setup a real
            authentication scheme before you could begin development.
            <!--#endif -->
          </v-card-text>
        </v-card>
        <br />
        <!--#if Tenancy -->
        <v-card title="Tenancy">
          <v-card-text>
            <p>
              You have created a multi-tenant Coalesce application. The tenancy
              model implemented in this template works as follows:
            </p>
            <br />
            <p>
              Every entity that exists within a tenant implements
              <code>ITenanted</code>.
            </p>
            <br />
            <p>
              Entity Framework model configuration is customized to place the
              <code>TenantId</code> as the first part of a composite primary key
              on <code>ITenanted</code> entities. Foreign keys between tenanted
              entities are updated to include the <code>TenantId</code>, which
              makes it 100% impossible for one tenant's data to become
              accidentally linked to another tenant's data.
            </p>
            <br />
            <p>
              Entity Framework is configured with a global query filter to
              automatically filter queries to the current tenant, and with
              interceptors and value generators to automatically populate the
              current TenantId on new entities.
            </p>
            <br />
            <p>
              Users exist independently of tenants, with their membership to
              zero-or-more tenants recorded in the
              <code>TenantMembership</code> table.
            </p>
            <br />
            <p>
              A user can have a special "GlobalAdmin" role, which could be
              utilized in your custom code to allow administration of all
              tenants.

              <!--#if TenantCreateAdmin -->
              Out of the box, the role allows for the creation of new tenants.
              <!--#endif  -->
            </p>
          </v-card-text>
        </v-card>
        <!--#endif -->
      </v-col>

      <v-col cols="12" md="4">
        <v-card>
          <v-card-title> Quick Links </v-card-title>
          <v-card-text>
            <v-list density="compact" class="doc-links">
              <v-list-item
                title="Coalesce Documentation"
                href="https://coalesce.intellitect.com"
                prepend-avatar="https://coalesce.intellitect.com/coalesce-icon-color.svg"
              >
              </v-list-item>
              <v-list-item
                title="Vue Documentation"
                href="https://vuejs.org/guide/introduction"
                prepend-avatar="https://raw.githubusercontent.com/vitejs/vite/9885f6f113667c1e161dee5f30af1e91aeefa62f/docs/images/vue.svg"
              >
              </v-list-item>
              <v-list-item
                title="Vuetify Documentation"
                href="https://vuetifyjs.com/en/"
                prepend-avatar="https://cdn.vuetifyjs.com/docs/images/logos/vuetify-logo-v3-slim-light.svg"
              ></v-list-item>
              <v-list-item
                title="Vite Documentation"
                href="https://vitejs.dev/"
                prepend-avatar="https://raw.githubusercontent.com/vitejs/vite/9885f6f113667c1e161dee5f30af1e91aeefa62f/docs/images/vite.svg"
              >
              </v-list-item>
            </v-list>
            <v-divider class="my-3" />
            <h2 class="headline font-weight-light mb-3">Resources</h2>
            <ul>
              <li>
                Check out
                <a href="https://github.com/vuejs/awesome-vue" target="_blank">
                  🎉 awesome-vue
                </a>
                for Vue components, libraries, examples, and more!
              </li>
            </ul>
          </v-card-text>
        </v-card>
        <br />
        <v-card>
          <v-card-title> Development Recommended Tooling: </v-card-title>
          <v-card-text>
            <div>
              <p>
                <b>Visual Studio</b> is recommended for back-end C# work, but
                other IDEs like Rider or VS Code can also be used.
              </p>
              Recommended extensions:
              <ul class="mt-1">
                <li>
                  <a
                    href="https://marketplace.visualstudio.com/items?itemName=MadsKristensen.NpmTaskRunner64"
                  >
                    NPM Task Runner
                  </a>
                  (for easily running
                  <code>dotnet coalesce</code>)
                </li>
              </ul>
            </div>
            <div class="mt-4">
              <b>VS Code</b> is recommended for front-end Vue/Typescript work,
              with these extensions:
              <ul class="mt-1">
                <li>
                  <a
                    href="https://marketplace.visualstudio.com/items?itemName=Vue.volar"
                  >
                    Vue - Official
                  </a>
                </li>
                <li>
                  <a
                    href="https://marketplace.visualstudio.com/items?itemName=antfu.goto-alias"
                  >
                    Goto definition alias
                  </a>
                </li>
                <li>
                  <a
                    href="https://marketplace.visualstudio.com/items?itemName=dbaeumer.vscode-eslint"
                  >
                    ESLint
                  </a>
                </li>
                <li>
                  <a
                    href="https://marketplace.visualstudio.com/items?itemName=esbenp.prettier-vscode"
                  >
                    Prettier
                  </a>
                </li>
              </ul>
            </div>
            <div class="mt-4">
              Recommended <b>browser extensions</b>:
              <ul class="mt-1">
                <li>
                  <a
                    href="https://devtools.vuejs.org/guide/installation.html"
                    target="_blank"
                  >
                    Vue Devtools
                  </a>
                </li>
              </ul>
            </div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
defineProps<{ msg: string | null }>();
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style lang="scss">
.browser-logos {
  position: relative;
  width: 64px;
  height: 64px;
  margin: auto;

  img {
    position: absolute;
    top: 0;
    animation-name: fade;
    animation-iteration-count: infinite;
    animation-duration: 6s;
    animation-timing-function: ease-in-out;
  }

  @keyframes fade {
    0% {
      opacity: 0;
    }

    20% {
      opacity: 1;
    }

    33% {
      opacity: 1;
    }

    53% {
      opacity: 0;
    }

    100% {
      opacity: 0;
    }
  }

  .f2 {
    animation-delay: -4s;
  }

  .f3 {
    animation-delay: -2s;
  }
}

.doc-links {
  .v-avatar {
    border-radius: 0;
  }

  img {
    object-fit: contain;
  }
}
</style>
