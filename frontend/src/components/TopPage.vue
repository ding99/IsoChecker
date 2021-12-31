<template>
  <div class="components-container">
    <split-pane @resize="resize" split="vertical">
      <template slot='paneL'>
        <div class="left-container">Left</div>
      </template>
      <template slot='paneR'>
        <split-pane split="horizontal">
          <template slot="paneL">
            <div class="top-container">Top</div>
          </template>
          <template slot="paneR">
            <div class="bottom-container">Bottom</div>
          </template>
        </split-pane>
      </template>
    </split-pane>
  </div>
</template>

<script>
import splitPane from 'vue-splitpane'

export default {
  name: 'HelloWorld',
  components: { splitPane },
  methods: {
    resize () {
      console.log('resize')
    }
  }
}
</script>

<style scoped>
  .components-containers {
    position: relative;
    height: 100vh;
  }
  .left-container {
    background-color: #681010;
    height: 100%;
  }
  .right-container {
    background-color: #f8e088;
    height: 200px;
  }
  .top-container {
    background-color: #f8e088;
    width: 100%;
    height: 100%;
  }
  .bottom-container {
    background-color: #086858;
    width: 100%;
    height: 100%;
  }
</style>
