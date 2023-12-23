<template>
  <div id="app">
    <nav>
      <router-link to="/shelters">Shelters</router-link>
      <router-link to="/pets">Pets</router-link>
    </nav>
    <router-view></router-view>
  </div>
</template>

<script>
export default {
  name: 'App',
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

nav {
  background-color: #f8f9fa;
  padding: 1em;
}

nav a {
  margin-right: 1em;
  text-decoration: none;
  color: black;
}
</style>
