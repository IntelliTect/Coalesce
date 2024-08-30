<template>
  <v-container>
    <v-row>
      <v-col cols="12" md="8">
        <v-card>
          <v-card-title>
            {{ msg }}
          </v-card-title>
          <v-card-text>
            <v-row
              style="max-width: 500px"
              class="mx-auto"
              justify="space-around"
              align="end"
            >
              <v-col>
                <a href="https://vuejs.org/">
                  <img
                    src="https://raw.githubusercontent.com/vitejs/vite/9885f6f113667c1e161dee5f30af1e91aeefa62f/docs/images/vue.svg"
                    width="100"
                  />
                </a>
              </v-col>
              <v-col>
                <a href="https://vuetifyjs.com/en/">
                  <img
                    src="https://cdn.vuetifyjs.com/docs/images/logos/vuetify-logo-v3-slim-light.svg"
                    width="100"
                  />
                </a>
              </v-col>
              <v-col>
                <a href="https://vitejs.dev/">
                  <img
                    src="https://raw.githubusercontent.com/vitejs/vite/9885f6f113667c1e161dee5f30af1e91aeefa62f/docs/images/vite.svg"
                    width="100"
                  />
                </a>
              </v-col>
            </v-row>
            <h2 class="headline font-weight-light my-3">
              Here are some next steps to get started:
            </h2>
            <ol>
              <li>
                <strong>Create your data model</strong> by adding classes to the
                Models folder in your data project, and adding DbSet properties
                on AppDbContext.
              </li>
              <li>
                <strong>Create an EF migration</strong> by running
                <code>dotnet ef migrations add MigrationName</code>
                in your .Data project. The pages below will not work until you
                do so.
              </li>
              <li>
                <i class="fas fa-star" />
                <strong> Implement a proper authentication mechanism</strong> in
                Program.cs and remove the existing dummy authentication. Options
                include
                <a
                  href="https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity"
                >
                  Identity
                </a>
                and
                <a
                  href="https://docs.microsoft.com/en-us/aspnet/core/security/authentication/windowsauth?view=aspnetcore-6.0#kestrel"
                >
                  Windows Authentication
                </a>

                <br />
                <br />
                This template authenticates all users as a dummy user to
                facilitate rapid initial development. Since all Coalesce
                endpoints require authentication by default, you would otherwise
                have to setup a real authentication scheme before you could
                begin development.
              </li>
              <li>
                <b>Check out the other pages</b> in the sidebar for
                <router-link to="/widget">examples</router-link> and
                <router-link to="/admin">admin page links</router-link>!
              </li>
            </ol>
            <v-divider class="my-4"></v-divider>
            <h2 class="headline font-weight-light my-3">
              Development Environment Recommended Tooling:
            </h2>
            <div style="max-width: 580px">
              <v-row align="center">
                <v-col style="flex-grow: 0">
                  <img
                    src="https://visualstudio.microsoft.com/wp-content/uploads/2021/10/Product-Icon.svg"
                    width="64"
                  />
                </v-col>
                <v-col>
                  Visual Studio is recommended for back-end C# work, but other
                  IDEs like Rider or VS Code can also be used. Recommended
                  extensions:
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
                </v-col>
              </v-row>
              <v-row align="center">
                <v-col style="flex-grow: 0">
                  <img
                    src="https://code.visualstudio.com/assets/images/code-stable.png"
                    width="64"
                  />
                </v-col>
                <v-col>
                  VS Code is recommended for front-end Vue/Typescript work, with
                  these extensions:
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
                </v-col>
              </v-row>
              <v-row>
                <v-col style="flex-grow: 0">
                  <div class="browser-logos">
                    <img
                      class="f1"
                      src="https://raw.githubusercontent.com/vuejs/devtools/1b1d1d0ce62e78b5cd8e628c89b31b6ed6df7a73/packages/docs/src/public/logo-chrome.svg"
                      width="64"
                    />
                    <img
                      class="f2"
                      src="https://raw.githubusercontent.com/vuejs/devtools/1b1d1d0ce62e78b5cd8e628c89b31b6ed6df7a73/packages/docs/src/public/logo-edge.png"
                      width="64"
                    />
                    <img
                      class="f3"
                      src="https://raw.githubusercontent.com/vuejs/devtools/1b1d1d0ce62e78b5cd8e628c89b31b6ed6df7a73/packages/docs/src/public/logo-firefox.svg"
                      width="64"
                    />
                  </div>
                </v-col>
                <v-col>
                  Recommended browser extensions:
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
                </v-col>
              </v-row>
            </div>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="4">
        <v-card>
          <v-card-title> Quick Links </v-card-title>
          <v-card-text>
            <h2 class="headline font-weight-light mb-3">Documentation</h2>
            <ul>
              <li>
                <a href="https://vuejs.org/guide/introduction">
                  Vue Documentation
                </a>
              </li>
              <li>
                <a href="https://vitejs.dev/"> Vite Documentation </a>
              </li>
              <li>
                <a href="https://vuetifyjs.com/en/getting-started/quick-start/">
                  Vuetify Documentation
                </a>
              </li>
              <li>
                <a
                  href="https://intellitect.github.io/Coalesce/stacks/vue/overview"
                >
                  Coalesce Documentation
                </a>
              </li>
              <li>
                <a
                  href="https://intellitect.github.io/Coalesce/stacks/vue/coalesce-vue-vuetify/overview"
                >
                  Coalesce Vuetify Documentation
                </a>
              </li>
            </ul>
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
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
defineProps<{ msg: string | null }>();
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped lang="scss">
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
</style>
