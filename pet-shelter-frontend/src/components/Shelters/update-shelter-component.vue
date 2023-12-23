<template>
  <div v-if="showModal" class="modal">
    <div class="modal-content">
      <span class="close" @click="showModal = false">&times;</span>
      <form @submit.prevent="submitForm">
        <label for="name">Name:</label>
        <input type="text" id="name" v-model="localShelter.name" required>
        <label for="location">Location:</label>
        <input type="text" id="location" v-model="localShelter.location" required>
        <label for="capacity">Capacity:</label>
        <input type="number" id="capacity" v-model="localShelter.capacity" min="0" required>
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
  props: ['shelter'],
  data() {
    return {
      showModal: false,
      response: null,
      localShelter: {} // Initialize localShelter as an empty object
    };
  },
  methods: {
    openModal(shelter) {
      this.localShelter = {
        id: shelter.id,
        name: shelter.name || '',
        location: shelter.location || '',
        capacity: shelter.capacity || ''
      };
      this.showModal = true;
    },
    async submitForm() {
      try {
        const response = await axios.put(`http://localhost:5000/api/Shelter/${this.localShelter.id}`, this.localShelter);
        this.response = response.data;
        this.showModal = false;
        this.$emit('shelterUpdated'); // Emit an event
      } catch (error) {
        console.error('Error submitting form:', error);
      }
    }
  },
  mounted() {
    this.$parent.updateShelterModal = this; // Assign this component instance to the parent component
  }
};
</script>
