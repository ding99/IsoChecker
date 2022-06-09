<template>
  <v-app id="app">
    <v-app-bar dark app heigth="48px">
      <div class="navbar">
        <img alt="iso viewer logo" src="./assets/iso32.png">
        <div class="ml-3">Iso Viewer</div>
      </div>
    </v-app-bar>
    <v-main>
      <ContainerRoot msg="Iso Viewer"/>
    </v-main>
  </v-app>
</template>

<script>
import ContainerRoot from './components/ContainerRoot.vue'

export default {
  name: 'App',
  components: {
    ContainerRoot
  }
}
</script>

<style scaped>
.navbar {
  display: flex;
  flex-direction: row;
}
</style>
