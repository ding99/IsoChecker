<template>
  <img alt="iso viewer logo" src="./assets/iso32.png">
  <ContainerRoot msg="Iso Viewer"/>
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

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}
</style>
