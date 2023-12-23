<template>
  <div v-if="isOpen" class="modal">
    <div class="modal-content">
      <span class="close" @click="closeModal">&times;</span>
      <p>Enter New Shelter ID:</p>
      <input type="text" v-model="newShelterId" placeholder="New Shelter ID">
      <button @click="submitChanges">Submit</button>
      <button @click="closeModal">Close</button>
    </div>
  </div>
</template>

<script>
export default {
  props: ['petId', 'oldShelterId'],
  data() {
    return {
      isOpen: false,
      newShelterId: '',
    };
  },
  methods: {
    openModal() {
      this.isOpen = true;
    },
    closeModal() {
      this.isOpen = false;
    },
    submitChanges() {
      this.$emit('shelterChanged', this.petId, this.newShelterId, this.oldShelterId);
      this.closeModal();
    },
  },
  mounted() {
    this.$parent.changeShelterModal = this;
  }
};
</script>
