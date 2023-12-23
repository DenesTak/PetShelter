<template>
  <div v-if="showModal" class="modal">
    <div class="modal-content">
      <span class="close" @click="showModal = false">&times;</span>
      <form @submit.prevent="submitForm">
        <label for="name">Name:</label>
        <input type="text" id="name" v-model="localPet.name" required>
        <label for="species">Species:</label>
        <input type="text" id="species" v-model="localPet.species" required>
        <label for="skin">Skin:</label>
        <input type="text" id="skin" v-model="localPet.skin" required>
        <label for="age">Age:</label>
        <input type="number" id="age" v-model="localPet.age" min="0" required>
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
  props: ['pet'],
  data() {
    return {
      showModal: false,
      response: null,
      localPet: {} // Initialize localPet as an empty object
    };
  },
  methods: {
    openModal(pet) {
      this.localPet = {
        id: pet.id,
        name: pet.name || '',
        species: pet.species || '',
        skin: pet.skin || '',
        age: pet.age || '',
      };
      this.showModal = true;
    },
    async submitForm() {
      try {
        const response = await axios.put(`http://localhost:5000/api/Pets/${this.localPet.id}`, this.localPet);
        this.response = response.data;
        this.showModal = false;
        this.$emit('petUpdated'); // Emit an event
      } catch (error) {
        console.error('Error submitting form:', error);
      }
    }
  },
  mounted() {
    this.$parent.updatePetModal = this; // Assign this component instance to the parent component
  }
};
</script>