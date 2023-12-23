<template>
  <h1>Pets</h1>
  <button @click="addPetModal.openModal()">Add Pet</button>
  <AddPetModal ref="addPetModal" />
  <button @click="sortByName">Sort by Name</button>
  <button @click="sortBySpecies">Sort by Species</button>
  <button @click="sortByAge">Sort by Age</button>
  <div>
    <label for="speciesDropdown">Filter by Species:</label>
    <select id="speciesDropdown" v-model="selectedSpecies" @change="updateFilteredPets">
      <option value="">All Species</option>
      <option v-for="species in uniqueSpecies" :key="species" :value="species">{{ species }}</option>
    </select>
  </div>
  <div class="container">
    <div v-for="(pet, id) in filteredPets" :key="id" class="card">
      <h3>{{ pet.name }}</h3>
      <button @click="openChangeShelterModal(pet.id, pet.shelterId)">Change Shelter</button>
      <ChangeShelterModal ref="changeShelterModal" :petId="pet.id" :oldShelterId="pet.shelterId" @shelterChanged="changeShelter" />
      <button @click="updatePet(pet.id)">Update</button>
      <UpdatePetModal ref="updatePetModal" @petUpdated="fetchData" />
      <button @click="deletePet(pet.id)">Delete</button>
      <div class="inner-box-container">
        <div v-for="(databasePet, databaseType) in pet.databases" :key="databaseType" class="inner-box">
          <h4>{{ databaseType }}</h4>
          <p>ID: {{ databasePet.id }}</p>
          <p>Species: {{ databasePet.species }}</p>
          <p>Skin: {{ databasePet.skin }}</p>
          <p>Age: {{ databasePet.age }}</p>
          <p>Shelter ID: {{ databasePet.shelterId }}</p>
          <p>Created Date: {{ databasePet.createdDate }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import AddPetModal from './add-pet-component.vue';
import UpdatePetModal from './update-pet-component.vue';
import ChangeShelterModal from './change-shelter-component.vue';

export default {
  components: {
    AddPetModal,
    UpdatePetModal,
    ChangeShelterModal
  },
  data() {
    return {
      currentPetId: null,
      addPetModal: null,
      changeShelterModal: null,
      UpdatePetModal: null,
      pets: {},
      sortKey: 'name',
      sortOrder: 'asc',
      selectedSpecies: "",
    };
  },
  computed: {
    sortedPets() {
      const sortedArray = Object.values(this.pets).sort((a, b) => {
        if (a[this.sortKey] < b[this.sortKey]) {
          return this.sortOrder === 'asc' ? -1 : 1;
        }
        if (a[this.sortKey] > b[this.sortKey]) {
          return this.sortOrder === 'asc' ? 1 : -1;
        }
        return 0;
      });
      return sortedArray;
    },
    uniqueSpecies() {
      // Calculate unique species from all pets
      const speciesSet = new Set();
      Object.values(this.pets).forEach((pet) => {
        speciesSet.add(pet.species);
      });
      return Array.from(speciesSet);
    },
    filteredPets() {
      if (this.selectedSpecies === "") {
        return this.sortedPets;
      }
      return Object.values(this.pets)
          .filter((pet) => pet.species === this.selectedSpecies)
          .sort((a, b) => {
            if (a[this.sortKey] < b[this.sortKey]) {
              return this.sortOrder === 'asc' ? -1 : 1;
            }
            if (a[this.sortKey] > b[this.sortKey]) {
              return this.sortOrder === 'asc' ? 1 : -1;
            }
            return 0;
          });
    },
  },
  watch: {
    selectedSpecies: 'updateFilteredPets',
  },
  methods: {
    updateFilteredPets() {
      // This method is called when selectedSpecies changes
      // You can add additional logic here if needed
    },
    async fetchData() {
      try {
        const response = await axios.get('http://localhost:5000/api/Pets', { timeout: 5000 });
        const data = response.data;

        let newStructure = {};

        for (let databaseType in data) {
          data[databaseType].forEach(pet => {
            if (!newStructure[pet.id]) {
              newStructure[pet.id] = {
                id: pet.id,
                name: pet.name,
                species: pet.species,
                skin: pet.skin,
                age: pet.age,
                shelterId: pet.shelterId,
                createdDate: pet.createdDate,
                databases: {
                  [databaseType]: pet
                }
              };
            } else {
              newStructure[pet.id].databases[databaseType] = pet;
            }
          });
        }

        this.pets = newStructure;
      } catch (error) {
        if (error.code === 'ECONNABORTED') {
          console.error('Request timed out');
        } else {
          console.error('Error fetching data:', error);
        }
      }
    },
    async changeShelter(petId, newShelterId, oldShelterId) {
      try {
        if (newShelterId) {
          await axios.post(`http://localhost:5000/api/Pets/AddToShelter`, {
            petId: petId,
            shelterId: newShelterId
          });
        } else {
          await axios.post(`http://localhost:5000/api/Pets/RemoveFromShelter`, {
            petId: petId,
            shelterId: oldShelterId
          });
        }
        this.fetchData();
      } catch (error) {
        console.error('Error changing shelter:', error);
      }
    },
    async deletePet(id) {
      try {
        await axios.delete(`http://localhost:5000/api/Pets/${id}`);
        await this.fetchData();
      } catch (error) {
        console.error('Error deleting pet:', error);
      }
    },
    openChangeShelterModal(petId, oldShelterId) {
      this.currentPetId = petId;
      this.oldShelterId = oldShelterId;
      this.changeShelterModal.openModal();
    },
    updatePet(id) {
      const pet = this.pets[id];
      this.updatePetModal.openModal(pet);
    },
    sortByName() {
      this.sortKey = 'name';
      this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
    },
    sortBySpecies() {
      this.sortKey = 'species';
      this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
    },
    sortByAge() {
      this.sortKey = 'age';
      this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
    },
  },
  mounted() {
    this.addPetModal = this.$refs.addPetModal;
    this.UpdatePetModal = this.$refs.updatePetModal;
    this.changeShelterModal = this.$refs.changeShelterModal;
    this.fetchData();
  },
};
</script>
