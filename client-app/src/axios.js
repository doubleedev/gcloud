import axios from 'axios';

console.log(window._env_)
const instance = axios.create({
    baseURL: window._env_.REACT_APP_API_URL + '/api/',
});

export default instance;
