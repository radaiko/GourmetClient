const mockGet = jest.fn();
const mockPostForm = jest.fn();
const mockPost = jest.fn();

jest.mock('axios', () => ({
  __esModule: true,
  default: {
    create: jest.fn(() => ({
      get: mockGet,
      postForm: mockPostForm,
      post: mockPost,
    })),
  },
}));

describe('GourmetHttpClient', () => {
  beforeEach(() => {
    jest.resetModules();
    jest.clearAllMocks();

    mockGet.mockResolvedValue({ data: '<html>test</html>', status: 200 });
    mockPostForm.mockResolvedValue({ data: '<html>post result</html>', status: 200 });
    mockPost.mockResolvedValue({ data: { success: true }, status: 200 });
  });

  function createClient() {
    const { GourmetHttpClient } = require('../../api/gourmetClient');
    return new GourmetHttpClient();
  }

  function getAxiosMock() {
    return require('axios').default;
  }

  describe('constructor', () => {
    it('creates axios instance with correct config', () => {
      createClient();
      const axios = getAxiosMock();

      expect(axios.create).toHaveBeenCalledWith(
        expect.objectContaining({
          baseURL: 'https://alaclickneu.gourmet.at',
          withCredentials: true,
          maxRedirects: 5,
        })
      );
    });
  });

  describe('get()', () => {
    it('calls client.get with url and params', async () => {
      const client = createClient();
      await client.get('/menus', { page: '0' });

      expect(mockGet).toHaveBeenCalledWith('/menus', {
        params: { page: '0' },
        responseType: 'text',
      });
    });

    it('returns response data', async () => {
      const client = createClient();
      const result = await client.get('/start/');

      expect(result).toBe('<html>test</html>');
    });
  });

  describe('postForm()', () => {
    it('calls client.postForm with url and data', async () => {
      const client = createClient();
      const data = { Username: 'user', Password: 'pass' };
      await client.postForm('/start/', data);

      expect(mockPostForm).toHaveBeenCalledWith(
        '/start/',
        data,
        expect.objectContaining({
          responseType: 'text',
        })
      );
    });

    it('sends Origin and Referer headers', async () => {
      const client = createClient();
      await client.postForm('/start/', { key: 'value' });

      expect(mockPostForm).toHaveBeenCalledWith(
        '/start/',
        expect.any(Object),
        expect.objectContaining({
          headers: expect.objectContaining({
            Origin: 'https://alaclickneu.gourmet.at',
            Referer: expect.any(String),
          }),
        })
      );
    });

    it('uses lastPageUrl as Referer after a GET', async () => {
      const client = createClient();
      await client.get('/start/');
      await client.postForm('/start/', { key: 'value' });

      expect(mockPostForm).toHaveBeenCalledWith(
        '/start/',
        expect.any(Object),
        expect.objectContaining({
          headers: expect.objectContaining({
            Referer: 'https://alaclickneu.gourmet.at/start/',
          }),
        })
      );
    });
  });

  describe('postJson()', () => {
    it('calls client.post with JSON content type', async () => {
      const client = createClient();
      const data = { eaterId: '123', shopModelId: '456' };
      await client.postJson('/umbraco/api/AlaCartApi/AddToMenuesCart', data);

      expect(mockPost).toHaveBeenCalledWith(
        '/umbraco/api/AlaCartApi/AddToMenuesCart',
        data,
        expect.objectContaining({
          headers: expect.objectContaining({
            'Content-Type': 'application/json',
          }),
        })
      );
    });

    it('sends Origin header', async () => {
      const client = createClient();
      await client.postJson('/api/test', { foo: 'bar' });

      expect(mockPost).toHaveBeenCalledWith(
        '/api/test',
        expect.any(Object),
        expect.objectContaining({
          headers: expect.objectContaining({
            Origin: 'https://alaclickneu.gourmet.at',
          }),
        })
      );
    });

    it('returns response data', async () => {
      const client = createClient();
      const result = await client.postJson('/api/test', {});

      expect(result).toEqual({ success: true });
    });
  });

  describe('resetClient()', () => {
    it('clears lastPageUrl so subsequent post uses url as Referer', async () => {
      const client = createClient();

      // GET sets lastPageUrl
      await client.get('/start/');
      // Reset clears it
      client.resetClient();
      // postForm should fall back to using the url itself as Referer
      await client.postForm('/bestellungen/', { key: 'value' });

      expect(mockPostForm).toHaveBeenCalledWith(
        '/bestellungen/',
        expect.any(Object),
        expect.objectContaining({
          headers: expect.objectContaining({
            Referer: '/bestellungen/',
          }),
        })
      );
    });
  });
});
