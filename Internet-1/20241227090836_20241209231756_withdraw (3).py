import hmac
import hashlib
import time
import requests
from urllib.parse import urlencode
from typing import Optional, Dict

class BinanceTRClient:
    def __init__(self, api_key: str, secret_key: str, base_url: str = 'https://www.binance.tr'):
        """
        Binance TR API istemcisi
        
        Args:
            api_key (str): API anahtarı
            secret_key (str): Gizli anahtar
            base_url (str): API base URL (varsayılan: 'https://www.trbinance.com')
        """
        self.api_key = api_key
        self.secret_key = secret_key.encode('utf-8')
        self.base_url = base_url

    def _generate_signature(self, params: Dict) -> str:
        """HMAC SHA256 imzası oluşturur"""
        query_string = urlencode(params)
        return hmac.new(
            self.secret_key,
            query_string.encode('utf-8'),
            hashlib.sha256
        ).hexdigest()

    def withdraw_try(self,
                    amount: float,
                    bank_account: Dict[str, str],
                    client_id: Optional[str] = None,
                    recv_window: int = 5000) -> Dict:
        """
        TRY para çekme işlemi yapar
        
        Args:
            amount (float): Çekilecek TRY miktarı
            bank_account (Dict[str, str]): Banka hesap bilgileri
                {
                    'account_holder': str,  # Hesap sahibinin tam adı
                    'iban': str,            # IBAN numarası
                    'bank_name': str,       # Banka adı
                    'bank_branch': str      # Şube adı/kodu (opsiyonel)
                }
            client_id (str, optional): Özel işlem ID'si
            recv_window (int, optional): Yanıt bekleme süresi (ms)

        Returns:
            Dict: API yanıtı
            
        Raises:
            BinanceTRException: API hatası durumunda
        """
        endpoint = '/open/v1/withdraws'  # Endpoint güncellendi
        
        # Temel parametreler
        params = {
            'asset': 'TRY',
            'amount': str(amount),
            'address': bank_account['iban'],
            'accountHolder': bank_account['account_holder'],
            'bankName': bank_account['bank_name'],
            'timestamp': str(int(time.time() * 1000))
        }

        # Opsiyonel parametreler
        if 'bank_branch' in bank_account:
            params['bankBranch'] = bank_account['bank_branch']
        if client_id:
            params['clientId'] = client_id
        if recv_window:
            params['recvWindow'] = recv_window

        # İmza oluştur ve ekle
        signature = self._generate_signature(params)
        params['signature'] = signature

        # API isteği gönder
        headers = {
            'X-MBX-APIKEY': self.api_key,
            'Content-Type': 'application/x-www-form-urlencoded'
        }

        try:
            response = requests.post(
                f"{self.base_url}{endpoint}",
                headers=headers,
                data=params
            )
            
            # Yanıt durumunu ve içeriğini yazdır (debug için)
            print(f"Status Code: {response.status_code}")
            print(f"Response Content: {response.text}")

            if response.status_code == 200:
                return response.json()
            else:
                raise BinanceTRException(f"API Hatası: {response.text}")
                
        except requests.RequestException as e:
            raise BinanceTRException(f"Bağlantı hatası: {str(e)}")


class BinanceTRException(Exception):
    """Binance TR API özel hata sınıfı"""
    pass


# Kullanım örneği
def main():
    # API anahtarlarınızı güvenli bir şekilde saklayın ve yönetin
    api_key = '716C9346Fb2F9eB99D947d4EB28F1479QCoroodqJHD7j9zU8LVLuSRXkFQe2DK5'
    secret_key = 'f4169612Fe36eE3a41643B3339166021X1xiYlP0hWuKSkGimaDXTGuSgMTSIKn2'

    # İstemci oluştur
    client = BinanceTRClient(api_key, secret_key)

    try:
        # TRY çekme isteği
        withdraw_request = client.withdraw_try(
            amount=1000.00,  # 1000 TRY
            bank_account={
                'account_holder': 'Ali Bayırlı',
                'iban': 'TR620001009010433243005001',
                'bank_name': 'Ziraat Bankası',
            }
        )

        print("Çekim talebi başarılı:")
        print(f"İşlem ID: {withdraw_request['data']['withdrawId']}")
        print(f"Timestamp: {withdraw_request['timestamp']}")

    except BinanceTRException as e:
        print(f"Çekim hatası: {e}")
    except Exception as e:
        print(f"Beklenmeyen hata: {e}")


if __name__ == "__main__":
    main()    
