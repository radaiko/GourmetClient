module.exports = {
  testEnvironment: 'node',
  testMatch: ['**/src-rn/__tests__/**/*.test.ts'],
  setupFilesAfterEnv: ['./src-rn/__tests__/setup.ts'],
  transform: {
    '^.+\\.tsx?$': ['ts-jest', {
      tsconfig: 'tsconfig.json',
    }],
  },
  moduleFileExtensions: ['ts', 'tsx', 'js', 'jsx', 'json'],
};
