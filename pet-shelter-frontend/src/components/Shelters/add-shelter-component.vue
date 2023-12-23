<template>
  <div v-if="showModal" class="modal">
    <div class="modal-content">
      <span class="close" @click="showModal = false">&times;</span>
      <form @submit.prevent="submitForm">
        <label for="name">Name:</label>
        <input type="text" id="name" v-model="shelter.name" required>
        <label for="location">Location:</label>
        <input type="text" id="location" v-model="shelter.location" required>
        <label for="capacity">Capacity:</label>
        <input type="number" id="capacity" v-model="shelter.capacity" min="0" required>
        <input type="submit" value="Submit">
      </form>
      <div v-if="response" class="success-message">
        {{ response }}
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';

export default {
  data() {
    return {
      showModal: false,
      shelter: {
        name: '',
        location: '',
        capacity: 0
      },
      response: null
    };
  },
  methods: {
    openModal() {
      this.showModal = true;
    },
    async submitForm() {
      try {
        const response = await axios.post('http://localhost:5000/api/Shelter', this.shelter);
        this.response = response.data;
        this.showModal = false;
        this.$emit('shelterAdded'); // Emit an event
      } catch (error) {
        console.error('Error submitting form:', error);
      }
    }
  }
};
</script>

<style>
.modal {
  position: fixed;
  z-index: 1;
  left: 0;
  top: 0;
  width: 100%;
  height: 100%;
  overflow: auto;
  background-color: rgba(0,0,0,0.4);
}

.modal-content {
  background-color: #fefefe;
  margin: 15% auto;
  padding: 20px;
  border: 1px solid #888;
  width: 80%;
}

.close {
  color: #aaa;
  float: right;
  font-size: 28px;
  font-weight: bold;
}

.close:hover,
.close:focus {
  color: black;
  text-decoration: none;
  cursor: pointer;
}
</style>
