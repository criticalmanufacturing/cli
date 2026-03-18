// Manual CJS mock for axios
// Needed because axios v1.x uses ES modules which Jest 27 (CommonJS mode) cannot auto-mock.
// jest.mock('axios') in test files will use this file instead of the real module.
const axios = jest.fn();
axios.create = jest.fn(() => axios);
module.exports = axios;
