import { VentopayTransaction } from '../types/ventopay';
import { generateDemoTransactions } from './demoData';

/**
 * Demo implementation of VentopayApi that returns fake data.
 * Same public interface as VentopayApi — no network requests.
 */
export class DemoVentopayApi {
  private loggedIn = false;

  async login(_username: string, _password: string): Promise<void> {
    this.loggedIn = true;
  }

  async getTransactions(fromDate: Date, toDate: Date): Promise<VentopayTransaction[]> {
    return generateDemoTransactions(fromDate, toDate);
  }

  async logout(): Promise<void> {
    this.loggedIn = false;
  }

  isAuthenticated(): boolean {
    return this.loggedIn;
  }
}
