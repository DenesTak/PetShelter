<template>
  <h1>Shelters</h1>
  <button @click="addShelterModal.openModal()">Add Shelter</button>
  <AddShelterModal ref="addShelterModal" />
  <button @click="sortByName">Sort by Name</button>
  <button @click="sortByLocation">Sort by Location</button>
  <button @click="sortByCapacity">Sort by Capacity</button>
  <div class="container">
    <div v-for="(shelter, id) in sortedShelters" :key="id" class="card">
      <h3>{{ shelter.name }}</h3>
      <button @click="updateShelter(shelter.id)">Update</button>
      <UpdateShelterModal ref="updateShelterModal" @shelterUpdated="fetchData" />
      <button @click="deleteShelter(shelter.id)">Delete</button>
      <div class="inner-box-container">
        <div v-for="(databaseShelter, databaseType) in shelter.databases" :key="databaseType" class="inner-box">
          <h4>{{ databaseType }}</h4>
          <p>ID: {{ databaseShelter.id }}</p>
          <p>Location: {{ databaseShelter.location }}</p>
          <p>Capacity: {{ databaseShelter.capacity }}</p>
          <div v-if="databaseShelter.pets">
            <h4>Pets in Shelter</h4>
            <div v-for="(pet, petIndex) in databaseShelter.pets" :key="petIndex" class="pets-card">
              <p>ID: {{ pet.id }}</p>
              <p>Name: {{ pet.name }}</p>
              <p>Species: {{ pet.species }}</p>
              <p>Skin: {{ pet.skin }}</p>
              <p>Age: {{ pet.age }}</p>
              <p>Shelter ID: {{ pet.shelterId }}</p>
              <p>Created Date: {{ pet.createdDate }}</p>
            </div>
          </div>
          <p>Created Date: {{ databaseShelter.createdDate }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import axios from 'axios';
import AddShelterModal from './add-shelter-component.vue';
import UpdateShelterModal from './update-shelter-component.vue';

export default {
  components: {
    AddShelterModal,
    UpdateShelterModal
  },
  data() {
    return {
      addShelterModal: null,
      UpdateShelterModal: null,
      shelters: {},
      sortKey: 'name',
      sortOrder: 'asc',
    };
  },
  computed: {
    sortedShelters() {
      const sortedArray = Object.values(this.shelters).sort((a, b) => {
        if (a[this.sortKey] < b[this.sortKey]) {
          return this.sortOrder === 'asc' ? -1 : 1;
        }
        if (a[this.sortKey] > b[this.sortKey]) {
          return this.sortOrder === 'asc' ? 1 : -1;
        }
        return 0;
      });
      return sortedArray;
    }
  },
  methods: {
    async fetchData() {
      try {
        const response = await axios.get('http://localhost:5000/api/Shelter', { timeout: 5000 });
        const data = response.data;

        let newStructure = {};

        for (let databaseType in data) {
          data[databaseType].forEach(shelter => {
            if (!newStructure[shelter.id]) {
              newStructure[shelter.id] = {
                id: shelter.id,
                name: shelter.name,
                location: shelter.location,
                capacity: shelter.capacity,
                databases: {
                  [databaseType]: shelter
                }
              };
            } else {
              newStructure[shelter.id].databases[databaseType] = shelter;
            }
          });
        }

        this.shelters = newStructure;
      } catch (error) {
        if (error.code === 'ECONNABORTED') {
          console.error('Request timed out');
        } else {
          console.error('Error fetching data:', error);
        }
      }
    },
    async deleteShelter(id) {
      try {
        await axios.delete(`http://localhost:5000/api/Shelter/${id}`);
        this.fetchData();
      } catch (error) {
        console.error('Error deleting shelter:', error);
      }
    },
    updateShelter(id) {
      const shelter = this.shelters[id];
      this.updateShelterModal.openModal(shelter);
    },
    sortByName() {
      this.sortKey = 'name';
      this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
    },
    sortByLocation() {
      this.sortKey = 'location';
      this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
    },
    sortByCapacity() {
      this.sortKey = 'capacity';
      this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
    },
  },
  mounted() {
    this.addShelterModal = this.$refs.addShelterModal;
    this.UpdateShelterModal = this.$refs.updateShelterModal;
    this.fetchData();
  },
};
</script>

<style>
.pets-card{
  border: 1px solid #000000;
  border-radius: 4px;
  padding: 1em;
  margin: 1em;
  box-sizing: border-box;
}
.container {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-around;
}

.card {
  border: 1px solid #ddd;
  border-radius: 4px;
  padding: 1em;
  margin: 1em;
  width: calc(50% - 2em);
  box-sizing: border-box;
}

.inner-box-container {
  display: flex;
  justify-content: space-between;
}
</style>
